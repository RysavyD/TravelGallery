import { useState } from 'react';
import {
  Alert,
  FlatList,
  Image,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { getTokens } from '@/lib/auth';
import { API_BASE_URL } from '@/lib/config';

interface SelectedImage {
  uri: string;
  fileName: string;
}

export default function UploadScreen() {
  const { id: tripId } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const [images, setImages] = useState<SelectedImage[]>([]);
  const [uploading, setUploading] = useState(false);
  const [progress, setProgress] = useState(0);

  const pickFromGallery = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      allowsMultipleSelection: true,
      quality: 0.8,
    });

    if (!result.canceled) {
      const newImages = result.assets.map(a => ({
        uri: a.uri,
        fileName: a.fileName || `photo_${Date.now()}.jpg`,
      }));
      setImages(prev => [...prev, ...newImages]);
    }
  };

  const takePhoto = async () => {
    const { status } = await ImagePicker.requestCameraPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Oprávnění zamítnuto', 'Pro focení je potřeba oprávnění k fotoaparátu.');
      return;
    }

    const result = await ImagePicker.launchCameraAsync({
      quality: 0.8,
    });

    if (!result.canceled && result.assets[0]) {
      const asset = result.assets[0];
      setImages(prev => [
        ...prev,
        {
          uri: asset.uri,
          fileName: asset.fileName || `photo_${Date.now()}.jpg`,
        },
      ]);
    }
  };

  const removeImage = (uri: string) => {
    setImages(prev => prev.filter(img => img.uri !== uri));
  };

  const handleUpload = async () => {
    if (images.length === 0) {
      Alert.alert('Chyba', 'Vyberte alespoň jednu fotku.');
      return;
    }

    setUploading(true);
    setProgress(0);

    const tokens = await getTokens();
    if (!tokens) {
      Alert.alert('Chyba', 'Nejste přihlášeni.');
      setUploading(false);
      return;
    }

    let completed = 0;
    let failed = 0;

    for (const img of images) {
      try {
        const formData = new FormData();
        formData.append('files', {
          uri: img.uri,
          name: img.fileName,
          type: 'image/jpeg',
        } as any);

        const res = await fetch(`${API_BASE_URL}/api/trips/${tripId}/media`, {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${tokens.accessToken}`,
          },
          body: formData,
        });

        if (!res.ok) {
          const text = await res.text().catch(() => '');
          console.error(`Upload failed (${res.status}): ${text}`);
          failed++;
          setProgress((completed + failed) / images.length);
          continue;
        }

        completed++;
        setProgress(completed / images.length);
      } catch (e) {
        console.error(`Upload failed for ${img.fileName}:`, e);
        failed++;
      }
    }

    setUploading(false);

    if (failed > 0) {
      Alert.alert(
        'Upload dokončen',
        `Úspěšně nahráno: ${completed}, selhalo: ${failed}`,
        [{ text: 'OK', onPress: () => router.back() }],
      );
    } else {
      Alert.alert('Hotovo', `Všech ${completed} fotek nahráno.`, [
        { text: 'OK', onPress: () => router.back() },
      ]);
    }
  };

  return (
    <View style={styles.container}>
      {/* Picker buttons */}
      <View style={styles.pickers}>
        <TouchableOpacity style={styles.pickerBtn} onPress={pickFromGallery}>
          <Ionicons name="images-outline" size={24} color="#2196F3" />
          <Text style={styles.pickerText}>Z galerie</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.pickerBtn} onPress={takePhoto}>
          <Ionicons name="camera-outline" size={24} color="#2196F3" />
          <Text style={styles.pickerText}>Vyfotit</Text>
        </TouchableOpacity>
      </View>

      {/* Selected images preview */}
      {images.length > 0 && (
        <Text style={styles.selectedCount}>Vybráno: {images.length} fotek</Text>
      )}

      <FlatList
        data={images}
        keyExtractor={item => item.uri}
        numColumns={3}
        renderItem={({ item }) => (
          <View style={styles.thumbContainer}>
            <Image source={{ uri: item.uri }} style={styles.thumb} />
            <TouchableOpacity
              style={styles.removeBtn}
              onPress={() => removeImage(item.uri)}
            >
              <Ionicons name="close-circle" size={22} color="#F44336" />
            </TouchableOpacity>
          </View>
        )}
        contentContainerStyle={styles.grid}
      />

      {/* Progress bar */}
      {uploading && (
        <View style={styles.progressContainer}>
          <View style={styles.progressTrack}>
            <View style={[styles.progressBar, { width: `${Math.round(progress * 100)}%` }]} />
          </View>
          <Text style={styles.progressText}>{Math.round(progress * 100)}%</Text>
        </View>
      )}

      {/* Upload button */}
      {images.length > 0 && (
        <TouchableOpacity
          style={[styles.uploadBtn, uploading && styles.uploadBtnDisabled]}
          onPress={handleUpload}
          disabled={uploading}
        >
          <Text style={styles.uploadBtnText}>
            {uploading ? 'Nahrávám...' : `Nahrát ${images.length} fotek`}
          </Text>
        </TouchableOpacity>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    padding: 16,
  },
  pickers: {
    flexDirection: 'row',
    gap: 12,
    marginBottom: 16,
  },
  pickerBtn: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    padding: 16,
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 12,
    backgroundColor: '#fafafa',
  },
  pickerText: {
    fontSize: 15,
    color: '#2196F3',
    fontWeight: '500',
  },
  selectedCount: {
    fontSize: 14,
    color: '#666',
    marginBottom: 8,
  },
  grid: {
    gap: 4,
  },
  thumbContainer: {
    position: 'relative',
    margin: 2,
  },
  thumb: {
    width: 110,
    height: 110,
    borderRadius: 8,
    backgroundColor: '#f0f0f0',
  },
  removeBtn: {
    position: 'absolute',
    top: 2,
    right: 2,
    backgroundColor: '#fff',
    borderRadius: 11,
  },
  progressContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    marginVertical: 12,
  },
  progressTrack: {
    flex: 1,
    height: 8,
    backgroundColor: '#e0e0e0',
    borderRadius: 4,
    overflow: 'hidden',
  },
  progressBar: {
    height: '100%',
    backgroundColor: '#2196F3',
    borderRadius: 4,
  },
  progressText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#333',
    width: 40,
    textAlign: 'right',
  },
  uploadBtn: {
    backgroundColor: '#2196F3',
    borderRadius: 8,
    padding: 16,
    alignItems: 'center',
    marginTop: 12,
  },
  uploadBtnDisabled: {
    opacity: 0.6,
  },
  uploadBtnText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});
