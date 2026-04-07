import { Alert } from 'react-native';

export function confirmDelete(
  title: string,
  message: string,
  onConfirm: () => void,
) {
  Alert.alert(title, message, [
    { text: 'Zrušit', style: 'cancel' },
    { text: 'Smazat', style: 'destructive', onPress: onConfirm },
  ]);
}
