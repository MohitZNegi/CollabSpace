import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { logout } from '../features/auth/authSlice';

function DashboardPage() {
    const { user } = useSelector((state) => state.auth);
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const handleLogout = () => {
        dispatch(logout());
        navigate('/login');
    };

    return (
        <div style={styles.container}>
            <nav style={styles.nav}>
                <span style={styles.navBrand}>CollabSpace</span>
                <div style={styles.navRight}>
                    <span style={styles.username}>
                        {user?.username}
                    </span>
                    <button style={styles.logoutBtn} onClick={handleLogout}>
                        Sign out
                    </button>
                </div>
            </nav>
            <main style={styles.main}>
                <h2>Welcome back, {user?.username}</h2>
                <p style={{ color: '#666' }}>
                    Your workspaces will appear here.
                </p>
            </main>
        </div>
    );
}

const styles = {
    container: { minHeight: '100vh', backgroundColor: '#f4f5f7' },
    nav: {
        backgroundColor: '#1B2A4A',
        padding: '1rem 2rem',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
    },
    navBrand: {
        color: '#ffffff',
        fontWeight: 600,
        fontSize: '1.2rem',
    },
    navRight: {
        display: 'flex',
        alignItems: 'center',
        gap: '1rem',
    },
    username: { color: '#a8c8e8', fontSize: '0.9rem' },
    logoutBtn: {
        backgroundColor: 'transparent',
        border: '1px solid #a8c8e8',
        color: '#a8c8e8',
        padding: '0.4rem 0.8rem',
        borderRadius: '4px',
        cursor: 'pointer',
        fontSize: '0.85rem',
    },
    main: {
        padding: '2rem',
        maxWidth: '1200px',
        margin: '0 auto',
    },
};

export default DashboardPage;