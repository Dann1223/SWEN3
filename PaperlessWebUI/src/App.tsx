import { Provider } from 'react-redux';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import { store } from './store';
import MainLayout from './components/Layout/MainLayout';
import ErrorBoundary from './components/common/ErrorBoundary';
import Dashboard from './pages/Dashboard';
import DocumentsPage from './pages/DocumentsPage';
import UploadPage from './pages/UploadPage';
import SearchPage from './pages/SearchPage';

function App() {
  return (
    <ErrorBoundary>
      <Provider store={store}>
        <ConfigProvider
          theme={{
            token: {
              colorPrimary: '#1890ff',
            },
          }}
        >
          <Router>
            <MainLayout>
              <Routes>
                <Route path="/" element={<Dashboard />} />
                <Route path="/documents" element={<DocumentsPage />} />
                <Route path="/upload" element={<UploadPage />} />
                <Route path="/search" element={<SearchPage />} />
              </Routes>
            </MainLayout>
          </Router>
        </ConfigProvider>
      </Provider>
    </ErrorBoundary>
  );
}

export default App;
