import { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { logout } from '../features/auth/authSlice';
import axiosInstance from '../api/axiosInstance';
import '../styles/components/dashboard.css';
import NotificationBell from '../components/NotificationBell';
import { DashboardPageSkeleton } from '../components/loading/PageSkeletons';


function DashboardPage() {
    const { user } = useSelector((state) => state.auth);
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const [workspaces, setWorkspaces] = useState([]);
    const [isLoadingWorkspaces, setIsLoadingWorkspaces] = useState(true);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showJoinModal, setShowJoinModal] = useState(false);
    const [createForm, setCreateForm] = useState({ name: '', description: '' });
    const [joinCode, setJoinCode] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Load workspaces when the component mounts
    useEffect(() => {
        fetchWorkspaces();
    }, []);

    const fetchWorkspaces = async () => {
        try {
            setIsLoadingWorkspaces(true);
            const response = await axiosInstance.get('/workspaces');
            setWorkspaces(response.data);
        } catch (error) {
            toast.error(
                error.response?.data?.error
                || 'Failed to create workspace.'
            );
        
        } finally {
            // finally runs whether the request succeeded or failed.
            // This guarantees the loading state is always cleared.
            setIsLoadingWorkspaces(false);
        }
    };

    const handleLogout = () => {
        dispatch(logout());
        toast.success('You have been signed out.');
        navigate('/login');
    };

    const handleCreateWorkspace = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            const response = await axiosInstance.post(
                '/workspaces', createForm);
            setWorkspaces((prev) => [...prev, response.data]);
            setShowCreateModal(false);
            setCreateForm({ name: '', description: '' });
            toast.success(`Workspace "${response.data.name}" created!`);
        } catch (error) {
            toast.error(
                error.response?.data?.error?.message
                || 'Failed to create workspace.'
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleJoinWorkspace = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            const response = await axiosInstance.post(
                '/workspaces/join', { inviteCode: joinCode.trim().toUpperCase() });

            // Only add to the list if not already there
            setWorkspaces((prev) => {
                const exists = prev.some((w) => w.id === response.data.id);
                return exists ? prev : [...prev, response.data];
            });

            setShowJoinModal(false);
            setJoinCode('');
            toast.success(`Joined "${response.data.name}" successfully!`);
        } catch (error) {
            toast.error(
                error.response?.data?.error
                || 'Invalid invite code. Please check and try again.'
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    const getRoleBadgeClass = (role) => {
        const map = {
            Owner: 'role-owner',
            Lead: 'role-lead',
            Member: 'role-member',
        };
        return `workspace-role-badge ${map[role] || 'role-member'}`;
    };

    if (isLoadingWorkspaces) {
        return <DashboardPageSkeleton />;
    }

    return (
        <div className="dashboard-container">

            {/* Navigation */}
            <nav className="dashboard-nav">
                <span className="nav-brand">CollabSpace</span>
                <div className="nav-right">
                    <span className="nav-username">{user?.username}</span>
                    <span className="nav-role-badge">{user?.globalRole}</span>
                    <NotificationBell />
                    <button className="nav-logout-btn" onClick={handleLogout}>
                        Sign out
                    </button>
                </div>
            </nav>

            {/* Main content */}
            <main className="dashboard-main">

                {/* Welcome banner */}
                <div className="dashboard-welcome">
                    <h2>Welcome back, {user?.username}</h2>
                    <p>
                        You have {workspaces.length} workspace
                        {workspaces.length !== 1 ? 's' : ''}.
                    </p>
                </div>

                {/* Workspaces section */}
                <div className="section-header">
                    <h3 className="section-title">Your Workspaces</h3>
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button
                            className="btn-secondary"
                            onClick={() => setShowJoinModal(true)}
                        >
                            Join workspace
                        </button>
                        <button
                            className="btn-primary"
                            onClick={() => setShowCreateModal(true)}
                        >
                            Create workspace
                        </button>
                    </div>
                </div>

                {workspaces.length === 0 ? (
                    <div className="empty-state">
                        <p>You are not part of any workspace yet.</p>
                        <button
                            className="btn-primary"
                            onClick={() => setShowCreateModal(true)}
                        >
                            Create your first workspace
                        </button>
                    </div>
                ) : (
                    <div className="workspace-grid">
                        {workspaces.map((workspace) => (
                            <div
                                key={workspace.id}
                                className="workspace-card"
                                onClick={() =>
                                    navigate(`/workspaces/${workspace.id}`)}
                            >
                                <div className="workspace-card-name">
                                    {workspace.name}
                                </div>
                                {workspace.description && (
                                    <div className="workspace-card-desc">
                                        {workspace.description}
                                    </div>
                                )}
                                <div className="workspace-card-footer">
                                    <span className={getRoleBadgeClass(
                                        workspace.workspaceRole)}>
                                        {workspace.workspaceRole}
                                    </span>
                                    <span style={{
                                        fontSize: '0.75rem',
                                        color: 'var(--color-text-muted)'
                                    }}>
                                        {new Date(workspace.createdAt)
                                            .toLocaleDateString()}
                                    </span>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </main>

            {/* Create Workspace Modal */}
            {showCreateModal && (
                <div className="modal-overlay"
                    onClick={() => setShowCreateModal(false)}>
                    <div className="modal"
                        onClick={(e) => e.stopPropagation()}>
                        <h3>Create a workspace</h3>
                        <form
                            onSubmit={handleCreateWorkspace}
                            className="modal-form"
                        >
                            <input
                                className="modal-input"
                                type="text"
                                placeholder="Workspace name"
                                value={createForm.name}
                                onChange={(e) => setCreateForm(
                                    (p) => ({ ...p, name: e.target.value }))}
                                required
                                maxLength={100}
                                autoFocus
                            />
                            <input
                                className="modal-input"
                                type="text"
                                placeholder="Description (optional)"
                                value={createForm.description}
                                onChange={(e) => setCreateForm(
                                    (p) => ({ ...p, description: e.target.value }))}
                                maxLength={500}
                            />
                            <div className="modal-actions">
                                <button
                                    type="button"
                                    className="btn-secondary"
                                    onClick={() => setShowCreateModal(false)}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    className="btn-primary"
                                    disabled={isSubmitting}
                                >
                                    {isSubmitting ? 'Creating...' : 'Create'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Join Workspace Modal */}
            {showJoinModal && (
                <div className="modal-overlay"
                    onClick={() => setShowJoinModal(false)}>
                    <div className="modal"
                        onClick={(e) => e.stopPropagation()}>
                        <h3>Join a workspace</h3>
                        <form
                            onSubmit={handleJoinWorkspace}
                            className="modal-form"
                        >
                            <input
                                className="modal-input"
                                type="text"
                                placeholder="Enter 8-character invite code"
                                value={joinCode}
                                onChange={(e) => setJoinCode(e.target.value)}
                                required
                                maxLength={8}
                                autoFocus
                                style={{
                                    textTransform: 'uppercase',
                                    letterSpacing: '0.15em'
                                }}
                            />
                            <div className="modal-actions">
                                <button
                                    type="button"
                                    className="btn-secondary"
                                    onClick={() => setShowJoinModal(false)}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    className="btn-primary"
                                    disabled={isSubmitting}
                                >
                                    {isSubmitting ? 'Joining...' : 'Join'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default DashboardPage;
