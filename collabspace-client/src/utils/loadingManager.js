let pendingRequests = 0;
const listeners = new Set();

function emitChange() {
    listeners.forEach((listener) => listener());
}

export function beginGlobalRequest() {
    pendingRequests += 1;
    emitChange();
}

export function endGlobalRequest() {
    pendingRequests = Math.max(0, pendingRequests - 1);
    emitChange();
}

export function subscribeToGlobalLoading(listener) {
    listeners.add(listener);
    return () => listeners.delete(listener);
}

export function getGlobalLoadingSnapshot() {
    return pendingRequests;
}
