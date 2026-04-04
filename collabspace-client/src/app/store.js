import { configureStore } from '@reduxjs/toolkit';
import authReducer from '../features/auth/authSlice';
import boardReducer from '../features/board/boardSlice';

const store = configureStore({
    reducer: {
        // Each key here becomes a section of your global state.
        // state.auth will hold everything from authSlice.
        auth: authReducer,
        board: boardReducer,
    },
});


export default store;