export interface Document {
  id: number;
  title: string;
  fileName: string;
  filePath: string;
  uploadDate: string;
  lastModified: string;
  fileType: string;
  fileSize: number;
  ocrText?: string;
  summary?: string;
  isProcessed: boolean;
  isIndexed: boolean;
  tags: Tag[];
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
  page: number;
  pageSize: number;
}

export interface DocumentUploadResponse {
  id: number;
  fileName: string;
  message: string;
}
