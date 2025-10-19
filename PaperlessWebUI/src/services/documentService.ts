import apiClient from './api';
import type { 
  Document, 
  Tag, 
  UpdateDocumentRequest,
  DocumentUploadResponse,
  DocumentSearchResult 
} from '../types/document.types';
import type { SearchRequest } from '../types/search.types';

export const documentService = {
  // Get all documents with pagination
  async getAll(page = 1, pageSize = 10): Promise<Document[]> {
    return apiClient.get(`/api/documents?page=${page}&pageSize=${pageSize}`);
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
    if (title) formData.append('title', title);
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

  // Search documents
  async search(request: SearchRequest): Promise<DocumentSearchResult> {
    const params = new URLSearchParams();
    
    if (request.query) params.append('query', request.query);
    if (request.page) params.append('page', request.page.toString());
    if (request.pageSize) params.append('pageSize', request.pageSize.toString());
    if (request.fileType) params.append('fileType', request.fileType);
    if (request.dateFrom) params.append('dateFrom', request.dateFrom);
    if (request.dateTo) params.append('dateTo', request.dateTo);
    if (request.sortBy) params.append('sortBy', request.sortBy);
    if (request.sortOrder) params.append('sortOrder', request.sortOrder);
    
    if (request.tagIds?.length) {
      request.tagIds.forEach(tagId => params.append('tagIds', tagId.toString()));
    }

    return apiClient.get(`/api/documents/search?${params.toString()}`);
  },

  // Get recent documents
  async getRecent(count = 5): Promise<Document[]> {
    return apiClient.get(`/api/documents/recent?count=${count}`);
  },

  // Download document
  async download(id: number): Promise<Blob> {
    const response = await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:8081'}/api/documents/${id}/download`);
    return response.blob();
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
