import { useCallback, useState } from 'react';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { apiFetch, AuthError } from '@/lib/api';
import { useAuth } from '@/hooks/useAuth';
import type { TripDto, TripListResponse } from '@/lib/types';
import TripCard from '@/components/TripCard';
import LoadingScreen from '@/components/LoadingScreen';

const PAGE_SIZE = 20;

export default function TripsListScreen() {
  const { isAuthenticated } = useAuth();
  const [trips, setTrips] = useState<TripDto[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchTrips = useCallback(async (pageNum: number, replace: boolean) => {
    try {
      const data = await apiFetch<TripListResponse>(
        `/api/trips?page=${pageNum}&pageSize=${PAGE_SIZE}`,
      );
      setTrips(prev => (replace ? data.data : [...prev, ...data.data]));
      setTotal(data.total);
      setPage(pageNum);
    } catch (e) {
      if (e instanceof AuthError) return; // Not logged in yet, ignore
      console.error('Failed to fetch trips:', e);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  // Reload on screen focus (e.g. after creating/editing a trip)
  useFocusEffect(
    useCallback(() => {
      if (!isAuthenticated) return;
      fetchTrips(1, true);
    }, [isAuthenticated, fetchTrips]),
  );

  const handleRefresh = () => {
    setRefreshing(true);
    fetchTrips(1, true);
  };

  const handleLoadMore = () => {
    if (trips.length < total) {
      fetchTrips(page + 1, false);
    }
  };

  if (loading) return <LoadingScreen />;

  return (
    <FlatList
      data={trips}
      keyExtractor={item => item.id.toString()}
      renderItem={({ item }) => <TripCard trip={item} />}
      onRefresh={handleRefresh}
      refreshing={refreshing}
      onEndReached={handleLoadMore}
      onEndReachedThreshold={0.5}
      contentContainerStyle={styles.list}
      ListEmptyComponent={
        <View style={styles.empty}>
          <Text style={styles.emptyText}>Žádné výlety</Text>
          <Text style={styles.emptyHint}>Přidejte první výlet přes záložku "Nový výlet"</Text>
        </View>
      }
    />
  );
}

const styles = StyleSheet.create({
  list: {
    paddingVertical: 8,
  },
  empty: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 100,
  },
  emptyText: {
    fontSize: 18,
    fontWeight: '600',
    color: '#999',
  },
  emptyHint: {
    fontSize: 14,
    color: '#bbb',
    marginTop: 8,
  },
});
