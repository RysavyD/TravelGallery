import { Stack } from 'expo-router';

export default function TripLayout() {
  return (
    <Stack
      screenOptions={{
        headerBackTitle: 'Zpět',
      }}
    >
      <Stack.Screen name="[id]" options={{ title: 'Detail výletu' }} />
      <Stack.Screen name="[id]/edit" options={{ title: 'Upravit výlet' }} />
      <Stack.Screen name="[id]/upload" options={{ title: 'Nahrát fotky' }} />
    </Stack>
  );
}
