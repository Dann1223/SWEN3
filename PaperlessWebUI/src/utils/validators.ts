import { SUPPORTED_FILE_TYPES, MAX_FILE_SIZE } from './constants';

export interface ValidationResult {
  isValid: boolean;
  message?: string;
}

export const validateFile = (file: File): ValidationResult => {
  // Check file type
  if (!SUPPORTED_FILE_TYPES.includes(file.type as any)) {
    return {
      isValid: false,
      message: `Unsupported file type: ${file.type}. Please upload PDF, Word, image, or text files.`,
    };
  }

  // Check file size
  if (file.size > MAX_FILE_SIZE) {
    return {
      isValid: false,
      message: `File too large: ${Math.round(file.size / 1024 / 1024)}MB. Maximum size is ${Math.round(MAX_FILE_SIZE / 1024 / 1024)}MB.`,
    };
  }

  return { isValid: true };
};

export const validateEmail = (email: string): ValidationResult => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  
  if (!email.trim()) {
    return { isValid: false, message: 'Email is required' };
  }
  
  if (!emailRegex.test(email)) {
    return { isValid: false, message: 'Please enter a valid email address' };
  }
  
  return { isValid: true };
};

export const validateRequired = (value: string, fieldName: string): ValidationResult => {
  if (!value?.trim()) {
    return { isValid: false, message: `${fieldName} is required` };
  }
  
  return { isValid: true };
};

export const validateMinLength = (value: string, minLength: number, fieldName: string): ValidationResult => {
  if (value.length < minLength) {
    return { isValid: false, message: `${fieldName} must be at least ${minLength} characters long` };
  }
  
  return { isValid: true };
};
