import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { loginUser, registerUser } from '../features/auth/authThunks';
import { clearError } from '../features/auth/authSlice';
import '../styles/components/auth.css';

function LoginPage() {
    const [mode, setMode] = useState('login');
    const [formData, setFormData] = useState({
        username: '',
        email: '',
        password: '',
    });

    const dispatch = useDispatch();
    const navigate = useNavigate();
    const { isLoading, error, user } = useSelector((state) => state.auth);

    // When the Redux store has a user, redirect to dashboard.
    // This fires whether login or register succeeded.
    // Using useEffect here is the correct React pattern because
    // navigation is a side effect of state changing.
    useEffect(() => {
        if (user) {
            navigate('/dashboard', { replace: true });
        }
    }, [user, navigate]);

    // Show backend error messages as toast notifications
    // whenever the error state changes.
    useEffect(() => {
        if (error) {
            toast.error(error);
            dispatch(clearError());
        }
    }, [error, dispatch]);

    const handleChange = (e) => {
        setFormData((prev) => ({
            ...prev,
            [e.target.name]: e.target.value,
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (mode === 'login') {
            const result = await dispatch(loginUser({
                email: formData.email,
                password: formData.password,
            }));

            // unwrap() throws on rejection, resolves on fulfilment.
            // This is the cleanest way to handle thunk results.
            if (loginUser.fulfilled.match(result)) {
                toast.success(`Welcome back, ${result.payload.user.username}!`);
                // Navigation handled by the useEffect above
            }

        } else {
            const result = await dispatch(registerUser({
                username: formData.username,
                email: formData.email,
                password: formData.password,
            }));

            if (registerUser.fulfilled.match(result)) {
                toast.success(`Account created! Welcome, ${result.payload.user.username}!`);
                // Navigation handled by the useEffect above
            }
        }
    };

    const handleModeSwitch = () => {
        dispatch(clearError());
        setFormData({ username: '', email: '', password: '' });
        setMode((prev) => (prev === 'login' ? 'register' : 'login'));
    };

    return (
        <div className="auth-container">
            <div className="auth-card">
                <h1 className="auth-logo">CollabSpace</h1>
                <h2 className="auth-subtitle">
                    {mode === 'login' ? 'Sign in to your account' : 'Create your account'}
                </h2>

                <form onSubmit={handleSubmit} className="auth-form">
                    {mode === 'register' && (
                        <div className="auth-field">
                            <label className="auth-label">Username</label>
                            <input
                                className="auth-input"
                                type="text"
                                name="username"
                                placeholder="Choose a username"
                                value={formData.username}
                                onChange={handleChange}
                                required
                                minLength={2}
                                maxLength={50}
                            />
                        </div>
                    )}

                    <div className="auth-field">
                        <label className="auth-label">Email address</label>
                        <input
                            className="auth-input"
                            type="email"
                            name="email"
                            placeholder="you@example.com"
                            value={formData.email}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div className="auth-field">
                        <label className="auth-label">Password</label>
                        <input
                            className="auth-input"
                            type="password"
                            name="password"
                            placeholder={mode === 'register'
                                ? 'Minimum 8 characters'
                                : 'Enter your password'}
                            value={formData.password}
                            onChange={handleChange}
                            required
                            minLength={8}
                        />
                    </div>

                    <button
                        className="auth-button"
                        type="submit"
                        disabled={isLoading}
                    >
                        {isLoading
                            ? 'Please wait...'
                            : mode === 'login' ? 'Sign in' : 'Create account'}
                    </button>
                </form>

                <button className="auth-switch" onClick={handleModeSwitch}>
                    {mode === 'login'
                        ? "Don't have an account? Register"
                        : 'Already have an account? Sign in'}
                </button>
            </div>
        </div>
    );
}

export default LoginPage;