import { useState } from 'react';
import { Alert, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import * as Location from 'expo-location';

interface Props {
  latitude?: number;
  longitude?: number;
  onLocationPicked: (coords: { latitude: number; longitude: number }) => void;
  onLocationCleared: () => void;
}

export default function LocationPicker({
  latitude,
  longitude,
  onLocationPicked,
  onLocationCleared,
}: Props) {
  const [loading, setLoading] = useState(false);

  const getLocation = async () => {
    setLoading(true);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Oprávnění zamítnuto', 'Pro získání polohy je potřeba oprávnění.');
        return;
      }

      const loc = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Balanced,
      });

      onLocationPicked({
        latitude: loc.coords.latitude,
        longitude: loc.coords.longitude,
      });
    } catch (e) {
      Alert.alert('Chyba', 'Nepodařilo se získat polohu.');
    } finally {
      setLoading(false);
    }
  };

  const hasLocation = latitude != null && longitude != null;

  return (
    <View style={styles.container}>
      <TouchableOpacity style={styles.button} onPress={getLocation} disabled={loading}>
        <Ionicons name="location-outline" size={20} color="#2196F3" />
        <Text style={styles.buttonText}>
          {loading ? 'Zjišťuji polohu...' : hasLocation ? 'Aktualizovat polohu' : 'Získat GPS polohu'}
        </Text>
      </TouchableOpacity>

      {hasLocation && (
        <View style={styles.coords}>
          <Text style={styles.coordText}>
            {latitude!.toFixed(6)}, {longitude!.toFixed(6)}
          </Text>
          <TouchableOpacity onPress={onLocationCleared}>
            <Ionicons name="close-circle" size={20} color="#999" />
          </TouchableOpacity>
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    marginBottom: 16,
  },
  button: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    padding: 12,
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 8,
    backgroundColor: '#fafafa',
  },
  buttonText: {
    fontSize: 15,
    color: '#2196F3',
  },
  coords: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: 8,
    paddingHorizontal: 4,
  },
  coordText: {
    fontSize: 13,
    color: '#666',
  },
});
