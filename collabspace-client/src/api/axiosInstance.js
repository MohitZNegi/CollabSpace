import axios from 'axios';
import {
    beginGlobalRequest,
    endGlobalRequest,
} from '../utils/loadingManager';

// One configured instance used everywhere in the app.
// Never call axios.get() directly in components.
// Always use this instance so interceptors apply everywhere.
const axiosInstance = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'https://localhost:5068/api/v1',
    headers: {
        'Content-Type': 'application/json',
    },
});

// REQUEST INTERCEPTOR
// Runs before every outgoing request.
// Reads the token from localStorage and attaches it to the
// Authorization header automatically. This means no component
// ever needs to manually add the token to a request.
axiosInstance.interceptors.request.use(
    (config) => {
        config.meta = config.meta || {};
        config.meta.showGlobalLoader = config.meta.showGlobalLoader !== false;

        if (config.meta.showGlobalLoader) {
            beginGlobalRequest();
        }

        const token = localStorage.getItem('accessToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        if (error.config?.meta?.showGlobalLoader) {
            endGlobalRequest();
        }
        return Promise.reject(error);
    }
);

// RESPONSE INTERCEPTOR
// Runs after every incoming response.
// If the server returns 401, the token has expired.
// Clear the stored tokens and redirect to login.
// This handles token expiry globally without any component
// needing to check for 401 errors themselves.
axiosInstance.interceptors.response.use(
    (response) => {
        if (response.config?.meta?.showGlobalLoader) {
            endGlobalRequest();
        }
        return response;
    },
    (error) => {
        if (error.config?.meta?.showGlobalLoader) {
            endGlobalRequest();
        }

        const isLoginRequest = error.config?.url?.includes('/auth/login')
            || error.config?.url?.includes('/auth/register');

        // Only redirect to login for 401s that are NOT from
        // the auth endpoints themselves. A 401 from login just
        // means wrong credentials, not an expired session.
        if (error.response?.status === 401 && !isLoginRequest) {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('user');
            window.location.href = '/login';
        }

        return Promise.reject(error);
    }
);

export default axiosInstance;
