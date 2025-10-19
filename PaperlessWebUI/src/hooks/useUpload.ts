import { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { useDocuments } from './useDocuments';

interface UseUploadOptions {
  maxFiles?: number;
  maxSize?: number; // in bytes
  acceptedFileTypes?: string[];
  onSuccess?: (files: File[]) => void;
  onError?: (error: string) => void;
}

export const useUpload = (options: UseUploadOptions = {}) => {
  const {
    maxFiles = 10,
    maxSize = 50 * 1024 * 1024, // 50MB
    acceptedFileTypes = [
      'application/pdf',
      'image/jpeg',
      'image/png',
      'image/gif',
      'text/plain',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    ],
    onSuccess,
    onError,
  } = options;

  const { uploadFile, uploadProgress, loading } = useDocuments();
  const [uploadQueue, setUploadQueue] = useState<File[]>([]);
  const [uploadErrors, setUploadErrors] = useState<string[]>([]);

  const validateFile = useCallback(
    (file: File): string | null => {
      if (file.size > maxSize) {
        return `File "${file.name}" is too large. Maximum size is ${Math.round(maxSize / 1024 / 1024)}MB.`;
      }

      if (acceptedFileTypes.length > 0 && !acceptedFileTypes.includes(file.type)) {
        return `File "${file.name}" has unsupported type. Supported types: ${acceptedFileTypes.join(', ')}.`;
      }

      return null;
    },
    [maxSize, acceptedFileTypes]
  );

  const processUpload = useCallback(
    async (files: File[], title?: string, tagIds?: number[]) => {
      setUploadErrors([]);
      const validFiles: File[] = [];
      const errors: string[] = [];

      // Validate all files
      files.forEach((file) => {
        const error = validateFile(file);
        if (error) {
          errors.push(error);
        } else {
          validFiles.push(file);
        }
      });

      if (errors.length > 0) {
        setUploadErrors(errors);
        onError?.(errors.join('\n'));
        return false;
      }

      if (validFiles.length === 0) {
        onError?.('No valid files to upload');
        return false;
      }

      // Upload files one by one
      try {
        for (const file of validFiles) {
          await uploadFile(file, title || file.name, tagIds);
        }
        
        onSuccess?.(validFiles);
        setUploadQueue([]);
        return true;
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : 'Upload failed';
        setUploadErrors([errorMessage]);
        onError?.(errorMessage);
        return false;
      }
    },
    [validateFile, uploadFile, onSuccess, onError]
  );

  const { getRootProps, getInputProps, isDragActive, acceptedFiles, fileRejections } = useDropzone({
    maxFiles,
    maxSize,
    accept: acceptedFileTypes.reduce((acc, type) => {
      acc[type] = [];
      return acc;
    }, {} as Record<string, string[]>),
    onDrop: (accepted, rejected) => {
      if (rejected.length > 0) {
        const errors = rejected.map(
          (rejection) => `${rejection.file.name}: ${rejection.errors.map(e => e.message).join(', ')}`
        );
        setUploadErrors(errors);
        onError?.(errors.join('\n'));
      }

      if (accepted.length > 0) {
        setUploadQueue(accepted);
      }
    },
  });

  const clearQueue = useCallback(() => {
    setUploadQueue([]);
    setUploadErrors([]);
  }, []);

  const removeFromQueue = useCallback((index: number) => {
    setUploadQueue(prev => prev.filter((_, i) => i !== index));
  }, []);

  return {
    // Dropzone props
    getRootProps,
    getInputProps,
    isDragActive,
    
    // State
    uploadQueue,
    uploadErrors,
    uploadProgress,
    isUploading: loading,
    acceptedFiles,
    fileRejections,
    
    // Actions
    processUpload,
    clearQueue,
    removeFromQueue,
    validateFile,
  };
};
