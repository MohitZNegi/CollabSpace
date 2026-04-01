import { useSelector } from 'react-redux';
import { Navigate, Outlet } from 'react-router-dom';

// Outlet renders whatever child route is nested inside this route.
// If the user is authenticated, render the child page.
// If not, redirect to login silently.
function ProtectedRoute() {
    const { user } = useSelector((state) => state.auth);

    return user ? <Outlet /> : <Navigate to="/login" replace />;
}

export default ProtectedRoute;