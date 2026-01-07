export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasMore: boolean;
  isFirstPage: boolean;
  isLastPage: boolean;
}

export interface Document {
  id: number;
  title: string;
  fileName: string;
  uploadDate: string;
  lastModified?: string;
  fileType: string;
  fileSize: number;
  ocrText?: string;
  summary?: string;
  isProcessed: boolean;
  isIndexed: boolean;
  tags?: Tag[];
}

export interface Tag {
  id: number;
  name: string;
  description?: string;
  color?: string;
  createdDate: string;
}

export interface DocumentAccess {
  id: number;
  documentId: number;
  accessDate: string;
  userAgent?: string;
  ipAddress?: string;
  actionType: string;
}

export interface CreateDocumentRequest {
  title: string;
  file: File;
  tagIds?: number[];
}

export interface UpdateDocumentRequest {
  title?: string;
  tagIds?: number[];
}

export interface DocumentSearchResult {
  documents: Document[];
  totalCount: number;
  searchTerm: string;
  searchDuration: {
    ticks: number;
    days: number;
    hours: number;
    milliseconds: number;
    minutes: number;
    seconds: number;
    totalDays: number;
    totalHours: number;
    totalMilliseconds: number;
    totalMinutes: number;
    totalSeconds: number;
  };
}

export interface DocumentUploadResponse {
  id: number;
  fileName: string;
  message: string;
}
