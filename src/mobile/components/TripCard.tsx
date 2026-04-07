import { Image, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { useRouter } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { format } from 'date-fns';
import { cs } from 'date-fns/locale';
import type { TripDto } from '@/lib/types';
import { API_BASE_URL } from '@/lib/config';

interface Props {
  trip: TripDto;
}

export default function TripCard({ trip }: Props) {
  const router = useRouter();

  const coverUrl = trip.coverPhotoUrl
    ? `${API_BASE_URL}/uploads/${trip.id}/thumbs/${trip.coverPhotoUrl}`
    : null;

  return (
    <TouchableOpacity
      style={styles.card}
      onPress={() => router.push(`/trip/${trip.id}`)}
      activeOpacity={0.7}
    >
      {coverUrl ? (
        <Image source={{ uri: coverUrl }} style={styles.cover} />
      ) : (
        <View style={[styles.cover, styles.placeholder]}>
          <Ionicons name="image-outline" size={40} color="#ccc" />
        </View>
      )}
      <View style={styles.info}>
        <Text style={styles.title} numberOfLines={1}>
          {trip.title}
        </Text>
        <Text style={styles.date}>
          {format(new Date(trip.date), 'd. MMMM yyyy', { locale: cs })}
        </Text>
        <View style={styles.meta}>
          <Ionicons name="camera-outline" size={14} color="#888" />
          <Text style={styles.metaText}>{trip.photoCount} fotek</Text>
          {trip.latitude != null && (
            <>
              <Ionicons name="location-outline" size={14} color="#888" style={{ marginLeft: 12 }} />
              <Text style={styles.metaText}>GPS</Text>
            </>
          )}
        </View>
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    backgroundColor: '#fff',
    borderRadius: 12,
    marginHorizontal: 16,
    marginVertical: 6,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.08,
    shadowRadius: 4,
    elevation: 2,
    overflow: 'hidden',
  },
  cover: {
    width: 100,
    height: 100,
  },
  placeholder: {
    backgroundColor: '#f0f0f0',
    justifyContent: 'center',
    alignItems: 'center',
  },
  info: {
    flex: 1,
    padding: 12,
    justifyContent: 'center',
  },
  title: {
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
    marginBottom: 4,
  },
  date: {
    fontSize: 13,
    color: '#666',
    marginBottom: 6,
  },
  meta: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  metaText: {
    fontSize: 12,
    color: '#888',
    marginLeft: 4,
  },
});
