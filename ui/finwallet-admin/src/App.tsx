import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ToastProvider } from './context/ToastContext';
import { ConfirmProvider } from './context/ConfirmContext';
import { Header } from './components/Header';
import { LoginPage } from './pages/LoginPage';
import { TotpPage } from './pages/TotpPage';
import { DashboardPage } from './pages/DashboardPage';

function ProtectedRoute({ children }: { children: JSX.Element }) {
  const { isLoggedIn } = useAuth();
  const location = useLocation();

  if (!isLoggedIn) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return children;
}

function AppLayout() {
  return (
    <div className="app-shell">
      <div className="bg-orb bg-orb--one" />
      <div className="bg-orb bg-orb--two" />
      <div className="bg-orb bg-orb--three" />
      <Header />
      <main className="relative px-5 pb-12 md:px-10">
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/totp"
            element={
              <ProtectedRoute>
                <TotpPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <ConfirmProvider>
          <AppLayout />
        </ConfirmProvider>
      </ToastProvider>
    </AuthProvider>
  );
}
