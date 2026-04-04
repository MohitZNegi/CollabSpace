import * as signalR from '@microsoft/signalr';

// A module-level variable holds the single connection instance.
// There should only ever be one SignalR connection per user session.
// This is the Singleton pattern applied to the frontend.
let connection = null;

// Builds the connection with the JWT token attached as a query parameter.
// The token factory is called every time a connection or reconnection
// happens, so it always uses the current token from localStorage.
export const buildConnection = () => {
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${import.meta.env.VITE_API_URL?.replace('/api/v1', '')}/hubs/collab`, {
            accessTokenFactory: () => localStorage.getItem('accessToken') || '',
        })
        // Automatic reconnection with increasing delays:
        // retry at 0s, 2s, 10s, 30s after disconnection.
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    return connection;
};

// Start the connection. Only builds if not already built.
export const startConnection = async () => {
    if (!connection) buildConnection();

    if (connection.state === signalR.HubConnectionState.Disconnected) {
        try {
            await connection.start();
            console.log('SignalR connected');
        } catch (error) {
            console.error('SignalR connection failed:', error);
        }
    }

    return connection;
};

// Stop and clean up the connection on logout.
export const stopConnection = async () => {
    if (connection) {
        await connection.stop();
        connection = null;
    }
};

// Join a board group so this client receives board events.
export const joinBoard = async (boardId) => {
    if (connection?.state === signalR.HubConnectionState.Connected) {
        await connection.invoke('JoinBoard', boardId);
    }
};

// Leave a board group when navigating away.
export const leaveBoard = async (boardId) => {
    if (connection?.state === signalR.HubConnectionState.Connected) {
        await connection.invoke('LeaveBoard', boardId);
    }
};

// Join a workspace group for chat and notification events.
export const joinWorkspace = async (workspaceId) => {
    if (connection?.state === signalR.HubConnectionState.Connected) {
        await connection.invoke('JoinWorkspace', workspaceId);
    }
};

export const leaveWorkspace = async (workspaceId) => {
    if (connection?.state === signalR.HubConnectionState.Connected) {
        await connection.invoke('LeaveWorkspace', workspaceId);
    }
};

// Register an event listener. Returns a cleanup function.
// Call the cleanup function when the component unmounts to
// prevent memory leaks from stale listeners.
export const onEvent = (eventName, handler) => {
    if (connection) {
        connection.on(eventName, handler);
        // Return cleanup function for use in useEffect
        return () => connection.off(eventName, handler);
    }
    return () => { };
};

export const getConnection = () => connection;