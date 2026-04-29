import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import GlobalLoader from './components/loading/GlobalLoader';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import ProtectedRoute from './components/ProtectedRoute';
import BoardPage from './pages/BoardPage';
import { useNotificationSignalR } from './hooks/useNotificationSignalR';
import WorkspacePage from './pages/WorkspacePage';

function AppRoutes() {
    return (
        <>
            <GlobalLoader />
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route element={<ProtectedRoute />}>
                    <Route path="/dashboard" element={<DashboardPage />} />
                </Route>
                <Route
                    path="/workspaces/:workspaceId"
                    element={<WorkspacePage />}
                />
                <Route
                    path="/workspaces/:workspaceId/boards/:boardId"
                    element={<BoardPage />}
                />
                <Route path="*" element={<Navigate to="/login" replace />} />
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
