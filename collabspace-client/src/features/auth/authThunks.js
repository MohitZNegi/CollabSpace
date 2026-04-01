import { createAsyncThunk } from '@reduxjs/toolkit';
import axiosInstance from '../../api/axiosInstance';


// createAsyncThunk handles the async lifecycle automatically.
// It dispatches pending, fulfilled, and rejected actions based
// on whether the promise resolves or rejects.
// The first argument is the action type prefix.
// The second argument is the async function (the payload creator).
export const loginUser = createAsyncThunk(
    'auth/login',
    async (credentials, { rejectWithValue }) => {
        try {
            const response = await axiosInstance.post(
                '/auth/login', credentials);

            const { accessToken, refreshToken, user } = response.data;

            // Persist tokens to localStorage so the session survives
            // a page refresh. The store re-reads these on initialisation.
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', refreshToken);
            localStorage.setItem('user', JSON.stringify(user));

            return { accessToken, user };
        } catch (error) {
            // rejectWithValue sends the error message to the rejected
            // action's payload, which authSlice stores in state.error.
            return rejectWithValue(
                error.response?.data?.error?.message
                || 'Login failed. Please try again.'
            );
        }
    }
);

export const registerUser = createAsyncThunk(
    'auth/register',
    async (userData, { rejectWithValue }) => {
        try {
            const response = await axiosInstance.post(
                '/auth/register', userData);

            const { accessToken, refreshToken, user } = response.data;

            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', refreshToken);
            localStorage.setItem('user', JSON.stringify(user));

            return { accessToken, user };
        } catch (error) {
            // Map specific backend messages to user-friendly ones.
            const serverMessage = error.response?.data?.error || '';

            let friendlyMessage = 'Registration failed. Please try again.';

           if (serverMessage.includes('Username') || serverMessage.includes('username'))
                friendlyMessage = 'That username is already taken. Please choose another one.';

            return rejectWithValue(friendlyMessage);
        }
    }
);