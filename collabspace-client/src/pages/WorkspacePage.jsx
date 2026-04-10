import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import toast from 'react-hot-toast';
import axiosInstance from '../api/axiosInstance';
import NotificationBell from '../components/NotificationBell';
import { logout } from '../features/auth/authSlice';
import '../styles/components/workspace.css';

// Maps action types to readable icon initials for the avatar
const ACTION_ICONS = {
    CardCreated: 'C+',
    CardMoved: '>>',
    CardUpdated: 'C~',
    CommentAdded: '"',
    MemberJoined: '+M',
    BoardCreated: 'B+',
    MessageSent: '~',
};

function WorkspacePage() {
    const { workspaceId } = useParams();
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const { user } = useSelector((s) => s.auth);

    const [workspace, setWorkspace] = useState(null);
    const [boards, setBoards] = useState([]);
    const [members, setMembers] = useState([]);
    const [dashboard, setDashboard] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [newBoardName, setNewBoardName] = useState('');
    const [isCreating, setIsCreating] = useState(false);

    useEffect(() => {
        const loadAll = async () => {
            try {
                setIsLoading(true);

                // Workspace must succeed — redirect if it fails
                const workspaceRes = await axiosInstance.get(
                    `/workspaces/${workspaceId}`);
                setWorkspace(workspaceRes.data);

                // Everything else loads independently
                const [boardsRes, membersRes, dashboardRes] =
                    await Promise.allSettled([
                        axiosInstance.get(
                            `/workspaces/${workspaceId}/boards`),
                        axiosInstance.get(
                            `/workspaces/${workspaceId}/members`),
                        axiosInstance.get(
                            `/workspaces/${workspaceId}/dashboard`),
                    ]);

                if (boardsRes.status === 'fulfilled')
                    setBoards(boardsRes.value.data);
                else toast.error('Failed to load boards.');

                if (membersRes.status === 'fulfilled')
                    setMembers(membersRes.value.data);
                else toast.error('Failed to load members.');

                if (dashboardRes.status === 'fulfilled')
                    setDashboard(dashboardRes.value.data);

            } catch (error) {
                console.error('Error loading workspace data:', error);
                toast.error('Workspace not found or access denied.');
                navigate('/dashboard');
            } finally {
                setIsLoading(false);
            }
        };

        loadAll();
    }, [workspaceId, navigate]);

    const handleCreateBoard = async (e) => {
        e.preventDefault();
        const name = newBoardName.trim();
        if (!name) return;

        setIsCreating(true);
        try {
            const response = await axiosInstance.post(
                `/workspaces/${workspaceId}/boards`, { name });
            setBoards((prev) => [...prev, response.data]);
            setShowCreateModal(false);
            setNewBoardName('');
            toast.success(`Board "${response.data.name}" created!`);
        } catch (error) {
            toast.error(
                error.response?.data?.error?.message
                || 'Failed to create board.');
        } finally {
            setIsCreating(false);
        }
    };

    const handleCopyInviteCode = () => {
        if (!workspace?.inviteCode) return;
        navigator.clipboard.writeText(workspace.inviteCode);
        toast.success('Invite code copied!');
    };

    const handleLogout = () => {
        dispatch(logout());
        navigate('/login');
    };

    const getRoleBadgeClass = (role) => {
        const map = {
            Owner: 'role-owner',
            Lead: 'role-lead',
            Member: 'role-member'
        };
        return `member-role-badge ${map[role] || 'role-member'}`;
    };

    const formatTime = (timestamp) => {
        const diff = Math.floor(
            (new Date() - new Date(timestamp)) / 60000);
        if (diff < 1) return 'Just now';
        if (diff < 60) return `${diff}m ago`;
        if (diff < 1440) return `${Math.floor(diff / 60)}h ago`;
        return new Date(timestamp).toLocaleDateString();
    };

    const isOnline = (lastSeenAt) => {
        if (!lastSeenAt) return false;
        return (new Date() - new Date(lastSeenAt)) < 15 * 60 * 1000;
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

                {/* Invite code */}
                {workspace?.inviteCode && (
                    <div className="invite-banner">
                        <div>
                            <div className="invite-banner-label">
                                Invite code
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

                {/* Task stats */}
                {dashboard?.taskStats && (
                    <div className="stats-bar">
                        <div className="stat-card todo">
                            <div className="stat-number">
                                {dashboard.taskStats.todo}
                            </div>
                            <div className="stat-label">To Do</div>
                        </div>
                        <div className="stat-card inprogress">
                            <div className="stat-number">
                                {dashboard.taskStats.inProgress}
                            </div>
                            <div className="stat-label">In Progress</div>
                        </div>
                        <div className="stat-card done">
                            <div className="stat-number">
                                {dashboard.taskStats.done}
                            </div>
                            <div className="stat-label">Done</div>
                        </div>
                    </div>
                )}

                {/* Active members strip */}
                {dashboard?.activeMembers?.length > 0 && (
                    <div className="active-members-strip">
                        <span className="active-members-label">
                            Recently active
                        </span>
                        {dashboard.activeMembers.map((member) => (
                            <div
                                key={member.userId}
                                className="active-member-chip"
                                title={member.lastSeenAt
                                    ? `Last seen ${formatTime(
                                        member.lastSeenAt)}`
                                    : 'Never seen'}
                            >
                                <div className="active-member-chip-avatar">
                                    {member.username[0].toUpperCase()}
                                </div>
                                {member.username}
                                {isOnline(member.lastSeenAt) && (
                                    <div className="online-dot"
                                        title="Online now" />
                                )}
                            </div>
                        ))}
                    </div>
                )}

                {/* Activity feed */}
                <div className="activity-feed">
                    <div className="activity-feed-header">
                        <h3 className="section-title">Recent Activity</h3>
                    </div>
                    <div className="activity-list">
                        {!dashboard?.recentActivity?.length ? (
                            <div className="activity-empty">
                                No activity yet. Create a board to get started.
                            </div>
                        ) : (
                            dashboard.recentActivity.map((item) => (
                                <div key={item.id} className="activity-item">
                                    <div className={`activity-avatar ${item.actionType}`}>
                                        {ACTION_ICONS[item.actionType]
                                            || item.actorUsername[0]
                                                .toUpperCase()}
                                    </div>
                                    <div className="activity-content">
                                        <p className="activity-description">
                                            {item.description}
                                        </p>
                                        <span className="activity-time">
                                            {formatTime(item.createdAt)}
                                        </span>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>

                {/* Boards */}
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
                    <button
                        className="board-card-create"
                        onClick={() => setShowCreateModal(true)}
                    >
                        <span className="board-card-create-icon">+</span>
                        <span>Create a board</span>
                    </button>
                </div>

                {/* Members */}
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