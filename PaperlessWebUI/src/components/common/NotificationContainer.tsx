import { useEffect } from 'react';
import { X } from 'lucide-react';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState } from '../../store';
import { removeNotification } from '../../store/slices/uiSlice';
import { cn } from '../../utils/helpers';

interface NotificationProps {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  autoClose?: boolean;
}

const NotificationItem = ({ id, type, title, message, autoClose = true }: NotificationProps) => {
  const dispatch = useDispatch();

  const typeStyles = {
    success: 'bg-green-50 border-green-200 text-green-800',
    error: 'bg-red-50 border-red-200 text-red-800',
    warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
    info: 'bg-blue-50 border-blue-200 text-blue-800',
  } as const;

  const iconColors = {
    success: 'text-green-400',
    error: 'text-red-400',
    warning: 'text-yellow-400',
    info: 'text-blue-400',
  } as const;

  useEffect(() => {
    if (!autoClose) return;
    const t = setTimeout(() => {
      dispatch(removeNotification(id));
    }, 5000);
    return () => clearTimeout(t);
  }, [autoClose, dispatch, id]);

  const handleClose = () => {
    dispatch(removeNotification(id));
  };

  return (
    <div
      className={cn(
        'max-w-sm w-full bg-white shadow-lg rounded-lg pointer-events-auto ring-1 ring-black ring-opacity-5 overflow-hidden',
        typeStyles[type]
      )}
    >
      <div className="p-4">
        <div className="flex items-start">
          <div className="flex-shrink-0">
            <div className={cn('w-6 h-6 rounded-full flex items-center justify-center', iconColors[type])}>
              {type === 'success' && '✓'}
              {type === 'error' && '✕'}
              {type === 'warning' && '⚠'}
              {type === 'info' && 'ℹ'}
            </div>
          </div>
          <div className="ml-3 w-0 flex-1 pt-0.5">
            <p className="text-sm font-medium">{title}</p>
            <p className="mt-1 text-sm opacity-90">{message}</p>
          </div>
          <div className="ml-4 flex-shrink-0 flex">
            <button
              className="bg-white rounded-md inline-flex text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              onClick={handleClose}
            >
              <X className="h-5 w-5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export const NotificationContainer = () => {
  const notifications = useSelector((state: RootState) => state.ui.notifications);
  if (notifications.length === 0) return null;

  return (
    <div className="fixed inset-0 flex items-end justify-center px-4 py-6 pointer-events-none sm:p-6 sm:items-start sm:justify-end z-50">
      <div className="w-full flex flex-col items-center space-y-4 sm:items-end">
        {notifications.map((n) => (
          <NotificationItem
            key={n.id}
            id={n.id}
            type={n.type}
            title={n.title}
            message={n.message}
            autoClose={n.autoClose}
          />
        ))}
      </div>
    </div>
  );
};
