import { useCallback, useState } from 'react';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
} from 'react-native';
import { useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router';
import { format } from 'date-fns';
import { apiFetch } from '@/lib/api';
import type { TripDetailResponse } from '@/lib/types';
import LocationPicker from '@/components/LocationPicker';
import DatePickerField from '@/components/DatePickerField';
import LoadingScreen from '@/components/LoadingScreen';

export default function EditTripScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const [title, setTitle] = useState('');
  const [date, setDate] = useState('');
  const [description, setDescription] = useState('');
  const [latitude, setLatitude] = useState<number | undefined>();
  const [longitude, setLongitude] = useState<number | undefined>();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useFocusEffect(
    useCallback(() => {
      (async () => {
        try {
          const data = await apiFetch<TripDetailResponse>(`/api/trips/${id}`);
          const t = data.trip;
          setTitle(t.title);
          setDate(format(new Date(t.date), 'yyyy-MM-dd'));
          setDescription(t.description || '');
          setLatitude(t.latitude ?? undefined);
          setLongitude(t.longitude ?? undefined);
        } catch (e) {
          Alert.alert('Chyba', 'Nepodařilo se načíst výlet.');
        } finally {
          setLoading(false);
        }
      })();
    }, [id]),
  );

  const handleSave = async () => {
    if (!title.trim()) {
      Alert.alert('Chyba', 'Název výletu je povinný.');
      return;
    }

    setSaving(true);
    try {
      await apiFetch(`/api/trips/${id}`, {
        method: 'PUT',
        body: JSON.stringify({
          title: title.trim(),
          date: new Date(date).toISOString(),
          description: description.trim() || undefined,
          latitude,
          longitude,
        }),
      });
      router.back();
    } catch (e: any) {
      Alert.alert('Chyba', e.message || 'Nepodařilo se uložit změny.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingScreen />;

  return (
    <KeyboardAvoidingView
      style={{ flex: 1 }}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <ScrollView style={styles.container} contentContainerStyle={styles.content}>
        <Text style={styles.label}>Název *</Text>
        <TextInput
          style={styles.input}
          value={title}
          onChangeText={setTitle}
          placeholder="Název výletu"
          maxLength={200}
        />

        <Text style={styles.label}>Datum *</Text>
        <DatePickerField value={date} onChange={setDate} />

        <Text style={styles.label}>Popis</Text>
        <TextInput
          style={[styles.input, styles.textArea]}
          value={description}
          onChangeText={setDescription}
          placeholder="Popis výletu (volitelné)"
          multiline
          numberOfLines={4}
          textAlignVertical="top"
        />

        <Text style={styles.label}>GPS poloha</Text>
        <LocationPicker
          latitude={latitude}
          longitude={longitude}
          onLocationPicked={({ latitude: lat, longitude: lng }) => {
            setLatitude(lat);
            setLongitude(lng);
          }}
          onLocationCleared={() => {
            setLatitude(undefined);
            setLongitude(undefined);
          }}
        />

        <TouchableOpacity
          style={[styles.saveBtn, saving && styles.saveBtnDisabled]}
          onPress={handleSave}
          disabled={saving}
        >
          <Text style={styles.saveBtnText}>
            {saving ? 'Ukládám...' : 'Uložit změny'}
          </Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  content: {
    padding: 16,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: '#333',
    marginBottom: 6,
    marginTop: 12,
  },
  input: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 8,
    padding: 12,
    fontSize: 16,
    backgroundColor: '#fafafa',
  },
  textArea: {
    minHeight: 100,
  },
  saveBtn: {
    backgroundColor: '#2196F3',
    borderRadius: 8,
    padding: 16,
    alignItems: 'center',
    marginTop: 24,
  },
  saveBtnDisabled: {
    opacity: 0.6,
  },
  saveBtnText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});
