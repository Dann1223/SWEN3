export const API_ENDPOINTS = {
  DOCUMENTS: '/api/documents',
  TAGS: '/api/tags',
  SEARCH: '/api/documents/search',
  UPLOAD: '/api/documents',
  HEALTH: '/health',
} as const;

export const FILE_TYPES = {
  PDF: 'application/pdf',
  WORD: 'application/msword',
  WORD_NEW: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  IMAGE_JPEG: 'image/jpeg',
  IMAGE_PNG: 'image/png',
  IMAGE_GIF: 'image/gif',
  TEXT: 'text/plain',
} as const;

export const SUPPORTED_FILE_TYPES = Object.values(FILE_TYPES);

export const MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB

export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 10,
  PAGE_SIZE_OPTIONS: ['10', '20', '50', '100'],
} as const;

export const UPLOAD_STATUS = {
  PENDING: 'pending',
  UPLOADING: 'uploading',
  SUCCESS: 'success',
  ERROR: 'error',
} as const;
