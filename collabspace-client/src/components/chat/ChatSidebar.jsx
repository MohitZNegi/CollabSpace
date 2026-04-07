import { useState, useEffect, useRef, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    setWorkspaceMessages
} from '../../features/chat/chatSlice';
import axiosInstance from '../../api/axiosInstance';
import { getConnection } from '../../services/signalrService';
import MessageBubble from './MessageBubble';
import '../../styles/components/chat.css';

function ChatSidebar({ workspaceId }) {
    const dispatch = useDispatch();
    const { user } = useSelector((state) => state.auth);
    const messages = useSelector(
        (state) => state.chat.workspaceMessages[workspaceId] || []);
    const typingUsers = useSelector(
        (state) => state.chat.typingUsers[workspaceId] || {});

    const [inputValue, setInputValue] = useState('');
    const [isSending, setIsSending] = useState(false);
    const scrollAnchorRef = useRef(null);
    const typingTimeoutRef = useRef(null);
    const inputRef = useRef(null);

    // Load message history when the sidebar mounts
    useEffect(() => {
        const loadMessages = async () => {
            try {
                const response = await axiosInstance.get(
                    `/workspaces/${workspaceId}/messages`);
                dispatch(setWorkspaceMessages({
                    workspaceId,
                    messages: response.data,
                }));
            } catch (error) {
                console.error('Failed to load messages:', error);
            }
        };

        loadMessages();
    }, [workspaceId, dispatch]);

    // Scroll to bottom whenever messages change.
    // scrollIntoView with behavior: 'smooth' animates the scroll,
    // making new messages feel natural rather than jumping.
    useEffect(() => {
        scrollAnchorRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    const handleSend = async () => {
        const content = inputValue.trim();
        if (!content || isSending) return;

        setIsSending(true);
        setInputValue('');

        try {
            // The POST saves to DB and triggers the SignalR broadcast.
            // The broadcast will fire workspaceMessageReceived in Redux,
            // which adds the message to the list for everyone including
            // the sender. The duplicate check in the slice prevents
            // it appearing twice.
            await axiosInstance.post(
                `/workspaces/${workspaceId}/messages`,
                { content });
        } catch (error) {
            // Log the error and restore the input if sending failed
            console.error('Failed to send message:', error);
            setInputValue(content);
        } finally {
            setIsSending(false);
            inputRef.current?.focus();
        }
    };

    // Send typing indicator via SignalR hub method.
    // Throttled: only fires once every 2 seconds while typing.
    // After 2 seconds of no typing, the timeout clears and the
    // next keystroke will send again.
    const handleTyping = useCallback(() => {
        if (typingTimeoutRef.current) return;

        const connection = getConnection();
        if (connection) {
            connection.invoke(
                'SendTypingIndicator',
                workspaceId,
                user?.username
            ).catch(() => { });
        }

        typingTimeoutRef.current = setTimeout(() => {
            typingTimeoutRef.current = null;
        }, 2000);
    }, [workspaceId, user?.username]);

    const handleKeyDown = (e) => {
        // Enter sends the message. Shift+Enter creates a new line.
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    };

    // Build the typing indicator text.
    // "Alice is typing..." or "Alice and Bob are typing..."
    const typingUsernames = Object.values(typingUsers);
    const typingText = typingUsernames.length === 0 ? ''
        : typingUsernames.length === 1
            ? `${typingUsernames[0]} is typing...`
            : `${typingUsernames.slice(0, -1).join(', ')} and ${typingUsernames.at(-1)} are typing...`;

    return (
        <div className="chat-sidebar">
            <div className="chat-header">Workspace Chat</div>

            <div className="chat-messages">
                {messages.map((message) => (
                    <MessageBubble
                        key={message.id}
                        message={message}
                        isOwn={message.senderId === user?.id}
                    />
                ))}
                {/* Invisible anchor — scrolled into view on new messages */}
                <div ref={scrollAnchorRef} className="chat-scroll-anchor" />
            </div>

            <div className="typing-indicator">{typingText}</div>

            <div className="chat-input-area">
                <textarea
                    ref={inputRef}
                    className="chat-input"
                    placeholder="Send a message..."
                    value={inputValue}
                    onChange={(e) => {
                        setInputValue(e.target.value);
                        handleTyping();
                    }}
                    onKeyDown={handleKeyDown}
                    rows={1}
                />
                <button
                    className="chat-send-btn"
                    onClick={handleSend}
                    disabled={!inputValue.trim() || isSending}
                >
                    &#9658;
                </button>
            </div>
        </div>
    );
}

export default ChatSidebar;