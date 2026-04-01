import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import store from './app/store';
import App from './App';
import './styles/global.css';
import { Toaster } from 'react-hot-toast';

// Provider makes the Redux store available to every component
// in the tree. Without it, useSelector and useDispatch do not work.
ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <Provider store={store}>
            <App />
            <Toaster
                position="top-right"
                toastOptions={{
                    duration: 4000,
                    className: 'custom-toast',
                    success: {
                        className: 'custom-toast custom-toast-success',
                    },
                    error: {
                        className: 'custom-toast custom-toast-error',
                    },
                }}
            />

        </Provider>
    </React.StrictMode>
);