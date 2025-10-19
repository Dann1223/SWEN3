import { useSelector, useDispatch } from 'react-redux';
import { useCallback } from 'react';
import type { RootState, AppDispatch } from '../store';
import {
  fetchDocuments,
  fetchDocumentById,
  uploadDocument,
  deleteDocument,
  fetchRecentDocuments,
  setSelectedDocument,
  clearError,
  clearUploadProgress,
} from '../store/slices/documentSlice';
import type { Document } from '../types/document.types';

export const useDocuments = () => {
  const dispatch = useDispatch<AppDispatch>();
  const {
    documents,
    selectedDocument,
    recentDocuments,
    loading,
    error,
    pagination,
    uploadProgress,
  } = useSelector((state: RootState) => state.documents);

  const loadDocuments = useCallback(
    (page = 1, pageSize = 10) => {
      dispatch(fetchDocuments({ page, pageSize }));
    },
    [dispatch]
  );

  const loadDocument = useCallback(
    (id: number) => {
      dispatch(fetchDocumentById(id));
    },
    [dispatch]
  );

  const uploadFile = useCallback(
    async (file: File, title?: string, tagIds?: number[]) => {
      try {
        await dispatch(uploadDocument({ file, title, tagIds }));
        // Refresh documents after upload
        dispatch(fetchDocuments({ page: 1, pageSize: pagination.pageSize }));
        return true;
      } catch (error) {
        console.error('Upload failed:', error);
        return false;
      }
    },
    [dispatch, pagination.pageSize]
  );

  const removeDocument = useCallback(
    async (id: number) => {
      try {
        await dispatch(deleteDocument(id));
        return true;
      } catch (error) {
        console.error('Delete failed:', error);
        return false;
      }
    },
    [dispatch]
  );

  const loadRecentDocuments = useCallback(
    (count?: number) => {
      dispatch(fetchRecentDocuments(count));
    },
    [dispatch]
  );

  const selectDocument = useCallback(
    (document: Document | null) => {
      dispatch(setSelectedDocument(document));
    },
    [dispatch]
  );

  const clearDocumentError = useCallback(() => {
    dispatch(clearError());
  }, [dispatch]);

  const resetUploadProgress = useCallback(() => {
    dispatch(clearUploadProgress());
  }, [dispatch]);

  return {
    documents,
    selectedDocument,
    recentDocuments,
    loading,
    error,
    pagination,
    uploadProgress,
    loadDocuments,
    loadDocument,
    uploadFile,
    removeDocument,
    loadRecentDocuments,
    selectDocument,
    clearDocumentError,
    resetUploadProgress,
  };
};
