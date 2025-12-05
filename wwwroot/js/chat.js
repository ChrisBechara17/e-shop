document.addEventListener('DOMContentLoaded', function () {
    const chatContainer = document.getElementById('chat-widget-container');
    const openBtn = document.getElementById('open-chat-btn');
    const closeBtn = document.getElementById('close-chat-btn');
    const sendBtn = document.getElementById('send-chat-btn');
    const input = document.getElementById('chat-input');
    const messagesContainer = document.getElementById('chat-messages');

    // Toggle Chat
    openBtn.addEventListener('click', () => {
        chatContainer.classList.remove('chat-widget-closed');
        openBtn.style.display = 'none';
        input.focus();
    });

    closeBtn.addEventListener('click', () => {
        chatContainer.classList.add('chat-widget-closed');
        openBtn.style.display = 'flex';
    });

    // Send Message
    function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        // Add User Message
        appendMessage(text, 'user-message');
        input.value = '';
        input.disabled = true;

        // Call API
        fetch('/api/chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: text })
        })
            .then(response => response.json())
            .then(data => {
                appendMessage(data.message, 'bot-message');
                input.disabled = false;
                input.focus();

                // Handle Actions
                if (data.actions && data.actions.length > 0) {
                    data.actions.forEach(action => {
                        if (action.type === 'add_to_cart') {
                            // Refresh cart count or show notification
                            // For simplicity, we can reload or update UI if we had comprehensive JS
                            const badge = document.querySelector('.badge');
                            if (badge) {
                                let count = parseInt(badge.innerText) || 0;
                                badge.innerText = count + action.data.quantity;
                            }
                            // Navigate to cart or show success toast
                            // window.location.href = '/Cart'; // Optional: auto-redirect
                        } else if (action.type === 'navigate') {
                            window.location.href = action.data.url;
                        }
                    });
                }
            })
            .catch(err => {
                console.error(err);
                appendMessage("Sorry, something went wrong.", 'bot-message');
                input.disabled = false;
            });
    }

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });

    function appendMessage(text, className) {
        const div = document.createElement('div');
        div.className = `chat-message ${className}`;
        div.innerText = text;
        messagesContainer.appendChild(div);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }
});
