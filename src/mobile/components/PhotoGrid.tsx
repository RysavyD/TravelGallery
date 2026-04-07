import { useState } from 'react';
import {
  Dimensions,
  FlatList,
  Image,
  StyleSheet,
  TouchableOpacity,
  View,
  Text,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import type { MediaDto } from '@/lib/types';
import PhotoViewer from './PhotoViewer';
import { confirmDelete } from './ConfirmDialog';

const NUM_COLUMNS = 3;
const GAP = 2;
const TILE_SIZE = (Dimensions.get('window').width - GAP * (NUM_COLUMNS + 1)) / NUM_COLUMNS;

interface Props {
  media: MediaDto[];
  onDeleteMedia?: (id: number) => void;
}

export default function PhotoGrid({ media, onDeleteMedia }: Props) {
  const [viewerUrl, setViewerUrl] = useState<string | null>(null);
  const [viewerCaption, setViewerCaption] = useState<string>('');

  const openViewer = (item: MediaDto) => {
    setViewerUrl(item.url);
    setViewerCaption(item.caption || '');
  };

  const handleLongPress = (item: MediaDto) => {
    if (!onDeleteMedia) return;
    confirmDelete('Smazat fotku?', `Opravdu chcete smazat "${item.fileName}"?`, () =>
      onDeleteMedia(item.id),
    );
  };

  if (media.length === 0) {
    return (
      <View style={styles.empty}>
        <Ionicons name="images-outline" size={48} color="#ccc" />
        <Text style={styles.emptyText}>Žádné fotky</Text>
      </View>
    );
  }

  return (
    <>
      <FlatList
        data={media}
        keyExtractor={item => item.id.toString()}
        numColumns={NUM_COLUMNS}
        scrollEnabled={false}
        renderItem={({ item }) => (
          <TouchableOpacity
            onPress={() => openViewer(item)}
            onLongPress={() => handleLongPress(item)}
            activeOpacity={0.8}
          >
            <Image source={{ uri: item.thumbnailUrl }} style={styles.tile} />
          </TouchableOpacity>
        )}
        contentContainerStyle={styles.grid}
      />
      <PhotoViewer
        visible={!!viewerUrl}
        imageUrl={viewerUrl}
        caption={viewerCaption}
        onClose={() => setViewerUrl(null)}
      />
    </>
  );
}

const styles = StyleSheet.create({
  grid: {
    padding: GAP,
  },
  tile: {
    width: TILE_SIZE,
    height: TILE_SIZE,
    margin: GAP / 2,
    borderRadius: 4,
    backgroundColor: '#f0f0f0',
  },
  empty: {
    alignItems: 'center',
    paddingVertical: 32,
  },
  emptyText: {
    fontSize: 14,
    color: '#999',
    marginTop: 8,
  },
});
