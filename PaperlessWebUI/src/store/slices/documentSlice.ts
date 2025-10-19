import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import { documentService } from '../../services/documentService';
import type { Document } from '../../types/document.types';
import type { SearchRequest } from '../../types/search.types';

interface DocumentState {
  documents: Document[];
  selectedDocument: Document | null;
  recentDocuments: Document[];
  loading: boolean;
  uploadProgress: number;
  error: string | null;
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

const initialState: DocumentState = {
  documents: [],
  selectedDocument: null,
  recentDocuments: [],
  loading: false,
  uploadProgress: 0,
  error: null,
  pagination: {
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  },
};

// Async thunks
export const fetchDocuments = createAsyncThunk(
  'documents/fetchDocuments',
  async ({ page = 1, pageSize = 10 }: { page?: number; pageSize?: number } = {}) => {
    const documents = await documentService.getAll(page, pageSize);
    return { documents, page, pageSize };
  }
);

export const fetchDocumentById = createAsyncThunk(
  'documents/fetchDocumentById',
  async (id: number) => {
    return await documentService.getById(id);
  }
);

export const uploadDocument = createAsyncThunk(
  'documents/uploadDocument',
  async (
    { file, title, tagIds }: { file: File; title?: string; tagIds?: number[] },
    { dispatch }
  ) => {
    const response = await documentService.upload(
      file,
      title,
      tagIds,
      (progress) => {
        dispatch(setUploadProgress(progress));
      }
    );
    return response;
  }
);

export const searchDocuments = createAsyncThunk(
  'documents/searchDocuments',
  async (request: SearchRequest) => {
    return await documentService.search(request);
  }
);

export const deleteDocument = createAsyncThunk(
  'documents/deleteDocument',
  async (id: number) => {
    await documentService.delete(id);
    return id;
  }
);

export const fetchRecentDocuments = createAsyncThunk(
  'documents/fetchRecentDocuments',
  async (count?: number) => {
    return await documentService.getRecent(count || 5);
  }
);

const documentSlice = createSlice({
  name: 'documents',
  initialState,
  reducers: {
    setSelectedDocument: (state, action: PayloadAction<Document | null>) => {
      state.selectedDocument = action.payload;
    },
    setUploadProgress: (state, action: PayloadAction<number>) => {
      state.uploadProgress = action.payload;
    },
    clearError: (state) => {
      state.error = null;
    },
    clearUploadProgress: (state) => {
      state.uploadProgress = 0;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch documents
      .addCase(fetchDocuments.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchDocuments.fulfilled, (state, action) => {
        state.loading = false;
        state.documents = action.payload.documents;
        state.pagination = {
          ...state.pagination,
          page: action.payload.page,
          pageSize: action.payload.pageSize,
        };
      })
      .addCase(fetchDocuments.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch documents';
      })
      
      // Fetch document by ID
      .addCase(fetchDocumentById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchDocumentById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedDocument = action.payload;
      })
      .addCase(fetchDocumentById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch document';
      })
      
      // Upload document
      .addCase(uploadDocument.pending, (state) => {
        state.loading = true;
        state.error = null;
        state.uploadProgress = 0;
      })
      .addCase(uploadDocument.fulfilled, (state) => {
        state.loading = false;
        state.uploadProgress = 100;
      })
      .addCase(uploadDocument.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to upload document';
        state.uploadProgress = 0;
      })
      
      // Search documents
      .addCase(searchDocuments.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(searchDocuments.fulfilled, (state, action) => {
        state.loading = false;
        state.documents = action.payload.documents;
        state.pagination = {
          page: action.payload.page,
          pageSize: action.payload.pageSize,
          totalCount: action.payload.totalCount,
          totalPages: Math.ceil(action.payload.totalCount / action.payload.pageSize),
        };
      })
      .addCase(searchDocuments.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to search documents';
      })
      
      // Delete document
      .addCase(deleteDocument.fulfilled, (state, action) => {
        state.documents = state.documents.filter(doc => doc.id !== action.payload);
      })
      
      // Fetch recent documents
      .addCase(fetchRecentDocuments.fulfilled, (state, action) => {
        state.recentDocuments = action.payload;
      });
  },
});

export const { 
  setSelectedDocument, 
  setUploadProgress, 
  clearError, 
  clearUploadProgress 
} = documentSlice.actions;

export default documentSlice.reducer;
