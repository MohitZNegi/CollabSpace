import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import toast from 'react-hot-toast';
import axiosInstance from '../api/axiosInstance';
import NotificationBell from '../components/NotificationBell';
import { logout } from '../features/auth/authSlice';
import { useDispatch } from 'react-redux';
import '../styles/components/workspace.css';

function WorkspacePage() {
    const { workspaceId } = useParams();
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const { user } = useSelector((s) => s.auth);

    const [workspace, setWorkspace] = useState(null);
    const [boards, setBoards] = useState([]);
    const [members, setMembers] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [newBoardName, setNewBoardName] = useState('');
    const [isCreating, setIsCreating] = useState(false);

    // Load workspace details, boards, and members in parallel.
    // Promise.all fires all three requests simultaneously rather
    // than waiting for each one before starting the next.
    // This cuts load time by roughly two thirds.
    useEffect(() => {
        const loadWorkspace = async () => {
            try {
                setIsLoading(true);

                const [workspaceRes, boardsRes, membersRes] = await Promise.all([
                    axiosInstance.get(`/workspaces/${workspaceId}`),
                    axiosInstance.get(`/workspaces/${workspaceId}/boards`),
                    axiosInstance.get(`/workspaces/${workspaceId}/members`),
                ]);

                setWorkspace(workspaceRes.data);
                setBoards(boardsRes.data);
                setMembers(membersRes.data);
            } catch (error) {
                console.error(error)
                toast.error('Failed to load workspace.');
                navigate('/dashboard');
            } finally {
                setIsLoading(false);
            }
        };

        loadWorkspace();
    }, [workspaceId, navigate]);

    const handleCreateBoard = async (e) => {
        e.preventDefault();
        const name = newBoardName.trim();
        if (!name) return;

        setIsCreating(true);
        try {
            const response = await axiosInstance.post(
                `/workspaces/${workspaceId}/boards`,
                { name });

            setBoards((prev) => [...prev, response.data]);
            setShowCreateModal(false);
            setNewBoardName('');
            toast.success(`Board "${response.data.name}" created!`);
        } catch (error) {
            toast.error(
                error.response?.data?.error?.message
                || 'Failed to create board.'
            );
        } finally {
            setIsCreating(false);
        }
    };

    const handleCopyInviteCode = () => {
        if (!workspace?.inviteCode) return;
        navigator.clipboard.writeText(workspace.inviteCode);
        toast.success('Invite code copied to clipboard!');
    };

    const handleLogout = () => {
        dispatch(logout());
        navigate('/login');
    };

    const getRoleBadgeClass = (role) => {
        const map = { Owner: 'role-owner', Lead: 'role-lead', Member: 'role-member' };
        return `member-role-badge ${map[role] || 'role-member'}`;
    };

    const formatDate = (timestamp) =>
        new Date(timestamp).toLocaleDateString('en-NZ', {
            year: 'numeric', month: 'short', day: 'numeric'
        });

    if (isLoading) {
        return (
            <div style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                height: '100vh',
                color: 'var(--color-text-muted)'
            }}>
                Loading workspace...
            </div>
        );
    }

    return (
        <div className="workspace-page">

            {/* Header */}
            <header className="workspace-header">
                <div className="workspace-header-left">
                    <button
                        className="workspace-back-btn"
                        onClick={() => navigate('/dashboard')}
                    >
                        &#8592; Dashboard
                    </button>
                    <span className="workspace-header-name">
                        {workspace?.name}
                    </span>
                </div>
                <div className="workspace-header-right">
                    <span style={{
                        color: 'var(--color-light-blue)',
                        fontSize: 'var(--font-size-sm)'
                    }}>
                        {user?.username}
                    </span>
                    <NotificationBell />
                    <button
                        className="nav-logout-btn"
                        onClick={handleLogout}
                    >
                        Sign out
                    </button>
                </div>
            </header>

            <main className="workspace-main">

                {/* Invite code banner */}
                {workspace?.inviteCode && (
                    <div className="invite-banner">
                        <div>
                            <div className="invite-banner-label">
                                Invite code — share this to add members
                            </div>
                            <div className="invite-code">
                                {workspace.inviteCode}
                            </div>
                        </div>
                        <button
                            className="invite-copy-btn"
                            onClick={handleCopyInviteCode}
                        >
                            Copy code
                        </button>
                    </div>
                )}

                {/* Boards section */}
                <div className="section-header">
                    <h3 className="section-title">Boards</h3>
                </div>

                <div className="boards-grid">
                    {boards.map((board) => (
                        <div
                            key={board.id}
                            className="board-card"
                            onClick={() => navigate(
                                `/workspaces/${workspaceId}/boards/${board.id}`
                            )}
                        >
                            <div className="board-card-name">
                                {board.name}
                            </div>
                            <div className="board-card-meta">
                                Created by {board.createdByUsername}
                                &nbsp;&middot;&nbsp;
                                {formatDate(board.createdAt)}
                            </div>
                        </div>
                    ))}

                    {/* Create new board tile */}
                    <button
                        className="board-card-create"
                        onClick={() => setShowCreateModal(true)}
                    >
                        <span className="board-card-create-icon">+</span>
                        <span>Create a board</span>
                    </button>
                </div>

                {/* Members section */}
                <div className="members-section">
                    <h3 className="section-title">
                        Members ({members.length})
                    </h3>
                    <div className="members-list">
                        {members.map((member) => (
                            <div key={member.userId} className="member-row">
                                <div className="member-avatar">
                                    {member.username[0].toUpperCase()}
                                </div>
                                <div className="member-info">
                                    <div className="member-username">
                                        {member.username}
                                        {member.userId === user?.id && (
                                            <span style={{
                                                marginLeft: '0.4rem',
                                                fontSize: '0.72rem',
                                                color: 'var(--color-text-muted)'
                                            }}>
                                                (you)
                                            </span>
                                        )}
                                    </div>
                                    <div className="member-joined">
                                        Joined {formatDate(member.joinedAt)}
                                    </div>
                                </div>
                                <span className={getRoleBadgeClass(
                                    member.workspaceRole)}>
                                    {member.workspaceRole}
                                </span>
                            </div>
                        ))}
                    </div>
                </div>
            </main>

            {/* Create Board Modal */}
            {showCreateModal && (
                <div
                    className="modal-overlay"
                    onClick={() => setShowCreateModal(false)}
                >
                    <div
                        className="modal"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <h3>Create a board</h3>
                        <form
                            onSubmit={handleCreateBoard}
                            className="modal-form"
                        >
                            <input
                                className="modal-input"
                                type="text"
                                placeholder="Board name"
                                value={newBoardName}
                                onChange={(e) =>
                                    setNewBoardName(e.target.value)}
                                required
                                maxLength={100}
                                autoFocus
                            />
                            <div className="modal-actions">
                                <button
                                    type="button"
                                    className="btn-secondary"
                                    onClick={() => {
                                        setShowCreateModal(false);
                                        setNewBoardName('');
                                    }}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="submit"
                                    className="btn-primary"
                                    disabled={isCreating
                                        || !newBoardName.trim()}
                                >
                                    {isCreating ? 'Creating...' : 'Create'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default WorkspacePage;