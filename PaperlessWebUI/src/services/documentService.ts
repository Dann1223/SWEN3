import apiClient from './api';
import type { 
  Document, 
  Tag, 
  UpdateDocumentRequest,
  DocumentUploadResponse,
  DocumentSearchResult,
  PaginatedResult
} from '../types/document.types';

export const documentService = {
  // Get all documents with pagination
  async getAll(page = 1, pageSize = 10): Promise<{ documents: Document[], totalCount: number, totalPages: number }> {
    const response = await apiClient.get(`/api/documents?page=${page}&pageSize=${pageSize}`) as PaginatedResult<Document>;
    return {
      documents: response.items || [],
      totalCount: response.totalCount || 0,
      totalPages: response.totalPages || 0
    };
  },

  // Get document by ID
  async getById(id: number): Promise<Document> {
    return apiClient.get(`/api/documents/${id}`);
  },

  // Upload new document
  async upload(
    file: File, 
    title?: string, 
    tagIds?: number[], 
    onProgress?: (progress: number) => void
  ): Promise<DocumentUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    // Always send title, use filename without extension as default
    const finalTitle = title?.trim() || file.name.replace(/\.[^/.]+$/, "");
    formData.append('title', finalTitle);
    if (tagIds?.length) {
      tagIds.forEach(tagId => formData.append('tagIds', tagId.toString()));
    }

    return apiClient.upload('/api/documents', formData, onProgress);
  },

  // Update document
  async update(id: number, data: UpdateDocumentRequest): Promise<Document> {
    return apiClient.put(`/api/documents/${id}`, data);
  },

  // Delete document
  async delete(id: number): Promise<void> {
    return apiClient.delete(`/api/documents/${id}`);
  },

  // Search documents (using the basic search endpoint)
  async search(query: string): Promise<DocumentSearchResult> {
    return apiClient.get(`/api/documents/search?query=${encodeURIComponent(query)}`);
  },

  // Get recent documents
  async getRecent(count = 5): Promise<Document[]> {
    return apiClient.get(`/api/documents/recent?count=${count}`);
  },

  // Download document
  async download(id: number): Promise<Blob> {
    const response = await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:8081'}/api/documents/${id}/download`);
    return response.blob();
  },

  // Get OCR processing status
  async getOcrStatus(id: number): Promise<any> {
    return apiClient.get(`/api/documents/${id}/ocr-status`);
  },

  // Get OCR text
  async getOcrText(id: number): Promise<any> {
    return apiClient.get(`/api/documents/${id}/ocr-text`);
  }
};

export const tagService = {
  // Get all tags
  async getAll(): Promise<Tag[]> {
    return apiClient.get('/api/tags');
  },

  // Get tag by ID
  async getById(id: number): Promise<Tag> {
    return apiClient.get(`/api/tags/${id}`);
  },

  // Create new tag
  async create(tag: Omit<Tag, 'id' | 'createdDate'>): Promise<Tag> {
    return apiClient.post('/api/tags', tag);
  },

  // Update tag
  async update(id: number, tag: Partial<Tag>): Promise<Tag> {
    return apiClient.put(`/api/tags/${id}`, tag);
  },

  // Delete tag
  async delete(id: number): Promise<void> {
    return apiClient.delete(`/api/tags/${id}`);
  }
};
