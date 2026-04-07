import { Image, Modal, StyleSheet, TouchableOpacity, View, Text, Dimensions } from 'react-native';
import { Ionicons } from '@expo/vector-icons';

const { width: SCREEN_W, height: SCREEN_H } = Dimensions.get('window');

interface Props {
  visible: boolean;
  imageUrl: string | null;
  caption?: string;
  onClose: () => void;
}

export default function PhotoViewer({ visible, imageUrl, caption, onClose }: Props) {
  if (!imageUrl) return null;

  return (
    <Modal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
      <View style={styles.overlay}>
        <TouchableOpacity style={styles.closeBtn} onPress={onClose}>
          <Ionicons name="close" size={28} color="#fff" />
        </TouchableOpacity>
        <Image
          source={{ uri: imageUrl }}
          style={styles.image}
          resizeMode="contain"
        />
        {caption ? <Text style={styles.caption}>{caption}</Text> : null}
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.95)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  closeBtn: {
    position: 'absolute',
    top: 50,
    right: 20,
    zIndex: 10,
    padding: 8,
  },
  image: {
    width: SCREEN_W,
    height: SCREEN_H * 0.7,
  },
  caption: {
    color: '#fff',
    fontSize: 14,
    marginTop: 16,
    paddingHorizontal: 24,
    textAlign: 'center',
  },
});
