export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface TripDto {
  id: number;
  title: string;
  date: string;
  description: string;
  latitude: number | null;
  longitude: number | null;
  photoCount: number;
  coverPhotoUrl: string | null;
}

export interface TripListResponse {
  total: number;
  page: number;
  pageSize: number;
  data: TripDto[];
}

export interface TripDetailResponse {
  trip: TripDto;
  media: MediaDto[];
}

export interface MediaDto {
  id: number;
  fileName: string;
  mediaType: 'Image' | 'Video';
  caption: string;
  sortOrder: number;
  url: string;
  thumbnailUrl: string;
  dateTaken: string | null;
  cameraModel: string | null;
  exifSummary: string | null;
}

export interface TripCreateDto {
  title: string;
  date: string;
  description?: string;
  latitude?: number;
  longitude?: number;
}
