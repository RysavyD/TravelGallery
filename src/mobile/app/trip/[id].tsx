import { useCallback, useState } from 'react';
import {
  Alert,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { Stack, useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { useWindowDimensions } from 'react-native';
import { format } from 'date-fns';
import RenderHtml from 'react-native-render-html';
import { cs } from 'date-fns/locale';
import MapView, { Marker } from 'react-native-maps';
import { apiFetch } from '@/lib/api';
import type { TripDetailResponse, TripDto, MediaDto } from '@/lib/types';
import PhotoGrid from '@/components/PhotoGrid';
import LoadingScreen from '@/components/LoadingScreen';
import { confirmDelete } from '@/components/ConfirmDialog';

export default function TripDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const { width: contentWidth } = useWindowDimensions();
  const [trip, setTrip] = useState<TripDto | null>(null);
  const [media, setMedia] = useState<MediaDto[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchDetail = useCallback(async () => {
    try {
      const data = await apiFetch<TripDetailResponse>(`/api/trips/${id}`);
      setTrip(data.trip);
      setMedia(data.media);
    } catch (e) {
      console.error('Failed to fetch trip detail:', e);
      Alert.alert('Chyba', 'Nepodařilo se načíst detail výletu.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useFocusEffect(
    useCallback(() => {
      fetchDetail();
    }, [fetchDetail]),
  );

  const handleDeleteTrip = () => {
    confirmDelete('Smazat výlet?', `Opravdu chcete smazat "${trip?.title}"?`, async () => {
      try {
        await apiFetch(`/api/trips/${id}`, { method: 'DELETE' });
        router.back();
      } catch (e) {
        Alert.alert('Chyba', 'Nepodařilo se smazat výlet.');
      }
    });
  };

  const handleDeleteMedia = async (mediaId: number) => {
    try {
      await apiFetch(`/api/trips/${id}/media/${mediaId}`, { method: 'DELETE' });
      setMedia(prev => prev.filter(m => m.id !== mediaId));
    } catch (e) {
      Alert.alert('Chyba', 'Nepodařilo se smazat fotku.');
    }
  };

  if (loading || !trip) return <LoadingScreen />;

  const hasGps = trip.latitude != null && trip.longitude != null;

  return (
    <ScrollView style={styles.container}>
      <Stack.Screen
        options={{
          title: trip.title,
          headerLeft: () => (
            <TouchableOpacity onPress={() => router.back()} style={{ marginRight: 8 }}>
              <Ionicons name="arrow-back" size={24} color="#2196F3" />
            </TouchableOpacity>
          ),
        }}
      />
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.title}>{trip.title}</Text>
        <Text style={styles.date}>
          {format(new Date(trip.date), 'd. MMMM yyyy', { locale: cs })}
        </Text>
      </View>

      {/* Actions */}
      <View style={styles.actions}>
        <TouchableOpacity
          style={styles.actionBtn}
          onPress={() => router.push(`/trip/${id}/edit`)}
        >
          <Ionicons name="create-outline" size={20} color="#2196F3" />
          <Text style={styles.actionText}>Upravit</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={styles.actionBtn}
          onPress={() => router.push(`/trip/${id}/upload`)}
        >
          <Ionicons name="cloud-upload-outline" size={20} color="#2196F3" />
          <Text style={styles.actionText}>Nahrát fotky</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.actionBtn} onPress={handleDeleteTrip}>
          <Ionicons name="trash-outline" size={20} color="#F44336" />
          <Text style={[styles.actionText, { color: '#F44336' }]}>Smazat</Text>
        </TouchableOpacity>
      </View>

      {/* Description */}
      {trip.description ? (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Popis</Text>
          <RenderHtml
            contentWidth={contentWidth - 32}
            source={{ html: trip.description }}
            baseStyle={{ fontSize: 14, color: '#555', lineHeight: 22 }}
          />
        </View>
      ) : null}

      {/* Map */}
      {hasGps ? (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Poloha</Text>
          <MapView
            style={styles.map}
            initialRegion={{
              latitude: trip.latitude!,
              longitude: trip.longitude!,
              latitudeDelta: 0.05,
              longitudeDelta: 0.05,
            }}
          >
            <Marker
              coordinate={{
                latitude: trip.latitude!,
                longitude: trip.longitude!,
              }}
              title={trip.title}
            />
          </MapView>
        </View>
      ) : null}

      {/* Photos */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Fotky ({media.length})</Text>
        <PhotoGrid media={media} onDeleteMedia={handleDeleteMedia} />
      </View>

      <View style={{ height: 40 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  header: {
    padding: 16,
    paddingBottom: 8,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#333',
  },
  date: {
    fontSize: 14,
    color: '#666',
    marginTop: 4,
  },
  actions: {
    flexDirection: 'row',
    paddingHorizontal: 16,
    paddingVertical: 8,
    gap: 12,
  },
  actionBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 8,
    backgroundColor: '#f5f5f5',
  },
  actionText: {
    fontSize: 13,
    color: '#2196F3',
    fontWeight: '500',
  },
  section: {
    paddingHorizontal: 16,
    paddingVertical: 12,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
    marginBottom: 8,
  },
  description: {
    fontSize: 14,
    color: '#555',
    lineHeight: 22,
  },
  map: {
    height: 200,
    borderRadius: 12,
    overflow: 'hidden',
  },
});
