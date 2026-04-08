import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import ProtectedRoute from './components/ProtectedRoute';
import BoardPage from './pages/BoardPage';
import { useNotificationSignalR } from './hooks/useNotificationSignalR';


function App() {
    // Active for the entire session regardless of current page
    useNotificationSignalR();
    return (
        <BrowserRouter>
            <Routes>
                {/* Public route — anyone can visit */}
                <Route path="/login" element={<LoginPage />} />

                {/* Protected routes — ProtectedRoute checks auth first */}
                <Route element={<ProtectedRoute />}>
                    <Route path="/dashboard" element={<DashboardPage />} />
                </Route>
                <Route
                    path="/workspaces/:workspaceId/boards/:boardId"
                    element={<BoardPage />}
                />
                {/* Default redirect */}
                <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;