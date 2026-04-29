import { useSyncExternalStore } from 'react';
import {
    subscribeToGlobalLoading,
    getGlobalLoadingSnapshot,
} from '../utils/loadingManager';

function useGlobalLoading() {
    const pendingRequests = useSyncExternalStore(
        subscribeToGlobalLoading,
        getGlobalLoadingSnapshot,
        getGlobalLoadingSnapshot
    );

    return {
        isLoading: pendingRequests > 4,
        pendingRequests,
    };
}

export default useGlobalLoading;
