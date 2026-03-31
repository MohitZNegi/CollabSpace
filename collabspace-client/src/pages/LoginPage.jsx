import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { loginUser, registerUser } from '../features/auth/authThunks';
import { clearError } from '../features/auth/authSlice';

function LoginPage() {
    // Toggle between Login and Register mode on the same page.
    // This matches the wireframe spec in your documentation.
    const [mode, setMode] = useState('login');
    const [formData, setFormData] = useState({
        username: '',
        email: '',
        password: '',
    });

    const dispatch = useDispatch();
    const navigate = useNavigate();
    const { isLoading, error } = useSelector((state) => state.auth);

    const handleChange = (e) => {
        setFormData((prev) => ({
            ...prev,
            [e.target.name]: e.target.value,
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Build the payload based on which mode we are in.
        const payload = mode === 'login'
            ? { email: formData.email, password: formData.password }
            : {
                username: formData.username,
                email: formData.email,
                password: formData.password
            };

        const action = mode === 'login'
            ? loginUser(payload)
            : registerUser(payload);

        // unwrap() throws if the thunk was rejected so we can
        // handle success and failure cleanly in one try/catch.
        const result = await dispatch(action);

        if (action.fulfilled && result.type.endsWith('/fulfilled')) {
            navigate('/dashboard');
        }
    };

    const handleModeSwitch = () => {
        dispatch(clearError());
        setFormData({ username: '', email: '', password: '' });
        setMode((prev) => (prev === 'login' ? 'register' : 'login'));
    };

    return (
        <div style={styles.container}>
            <div style={styles.card}>
                <h1 style={styles.logo}>CollabSpace</h1>
                <h2 style={styles.title}>
                    {mode === 'login' ? 'Sign in' : 'Create account'}
                </h2>

                {/* Error banner — only shown when there is an error */}
                {error && (
                    <div style={styles.errorBanner}>
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit} style={styles.form}>
                    {mode === 'register' && (
                        <input
                            style={styles.input}
                            type="text"
                            name="username"
                            placeholder="Username"
                            value={formData.username}
                            onChange={handleChange}
                            required
                        />
                    )}
                    <input
                        style={styles.input}
                        type="email"
                        name="email"
                        placeholder="Email address"
                        value={formData.email}
                        onChange={handleChange}
                        required
                    />
                    <input
                        style={styles.input}
                        type="password"
                        name="password"
                        placeholder="Password"
                        value={formData.password}
                        onChange={handleChange}
                        required
                        minLength={8}
                    />
                    <button
                        style={{
                            ...styles.button,
                            opacity: isLoading ? 0.7 : 1,
                        }}
                        type="submit"
                        disabled={isLoading}
                    >
                        {isLoading
                            ? 'Please wait...'
                            : mode === 'login' ? 'Sign in' : 'Create account'}
                    </button>
                </form>

                <button style={styles.switchButton} onClick={handleModeSwitch}>
                    {mode === 'login'
                        ? "Don't have an account? Register"
                        : 'Already have an account? Sign in'}
                </button>
            </div>
        </div>
    );
}

// Inline styles keep this component self-contained.
// In a larger project you would use CSS modules or Tailwind.
const styles = {
    container: {
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: '#f4f5f7',
    },
    card: {
        backgroundColor: '#ffffff',
        padding: '2.5rem',
        borderRadius: '8px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        width: '100%',
        maxWidth: '420px',
    },
    logo: {
        textAlign: 'center',
        color: '#1B2A4A',
        marginBottom: '0.25rem',
        fontSize: '1.8rem',
    },
    title: {
        textAlign: 'center',
        color: '#555',
        fontWeight: 400,
        marginBottom: '1.5rem',
        fontSize: '1rem',
    },
    errorBanner: {
        backgroundColor: '#FDE8E8',
        color: '#8B1A1A',
        padding: '0.75rem 1rem',
        borderRadius: '4px',
        marginBottom: '1rem',
        fontSize: '0.9rem',
    },
    form: {
        display: 'flex',
        flexDirection: 'column',
        gap: '0.75rem',
    },
    input: {
        padding: '0.75rem 1rem',
        borderRadius: '4px',
        border: '1px solid #ddd',
        fontSize: '1rem',
        outline: 'none',
    },
    button: {
        padding: '0.75rem',
        backgroundColor: '#1B2A4A',
        color: '#ffffff',
        border: 'none',
        borderRadius: '4px',
        fontSize: '1rem',
        cursor: 'pointer',
        marginTop: '0.5rem',
    },
    switchButton: {
        background: 'none',
        border: 'none',
        color: '#2E5FA3',
        cursor: 'pointer',
        fontSize: '0.9rem',
        marginTop: '1.25rem',
        width: '100%',
        textAlign: 'center',
    },
};

export default LoginPage;