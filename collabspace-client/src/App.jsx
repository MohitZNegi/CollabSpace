import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import GlobalLoader from './components/loading/GlobalLoader';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import ProtectedRoute from './components/ProtectedRoute';
import BoardPage from './pages/BoardPage';
import { useNotificationSignalR } from './hooks/useNotificationSignalR';
import WorkspacePage from './pages/WorkspacePage';
import LandingPage from './pages/LandingPage';
import AboutPage from './pages/AboutPage';

function AppRoutes() {
    return (
        <>
            <GlobalLoader />
                <Routes>
                    {/* Public routes */}
                    <Route path="/" element={<LandingPage />} />
                    <Route path="/about" element={<AboutPage />} />
                    <Route path="/login" element={<LoginPage />} />

                    {/* Protected routes */}
                    <Route element={<ProtectedRoute />}>
                        <Route path="/dashboard"
                            element={<DashboardPage />} />
                        <Route path="/workspaces/:workspaceId"
                            element={<WorkspacePage />} />
                        <Route
                            path="/workspaces/:workspaceId/boards/:boardId"
                            element={<BoardPage />} />
                    </Route>

                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>            
        </>
    );
}

function App() {
    useNotificationSignalR();

    return (
        <BrowserRouter>
            <AppRoutes />
        </BrowserRouter>
    );
}

export default App;
