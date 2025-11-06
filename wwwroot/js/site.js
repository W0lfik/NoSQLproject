
const escapeSelector = (value) => {
    if (window.CSS && typeof window.CSS.escape === 'function') {
        return window.CSS.escape(value);
    }
    return String(value).replace(/([ #;?%&,.+*~\':"!^$[\]()=>|/@])/g, '\\$1');
};

const initPrioritySelector = () => {
    const hiddenPriorityInput = document.getElementById('HiddenPriority');
    if (!hiddenPriorityInput) {
        return;
    }

    const priorityButtons = Array.from(document.querySelectorAll('.priority-btn'));
    if (!priorityButtons.length) {
        return;
    }

    const setActivePriority = (value) => {
        priorityButtons.forEach((btn) => {
            const isActive = btn.dataset.priorityName === value;
            btn.classList.toggle('active', isActive);
        });
        if (value) {
            hiddenPriorityInput.value = value;
        }
    };

    const initialPriority = hiddenPriorityInput.dataset.initialPriority || hiddenPriorityInput.value;
    if (initialPriority) {
        setActivePriority(initialPriority);
    }

    priorityButtons.forEach((button) => {
        button.addEventListener('click', () => {
            setActivePriority(button.dataset.priorityName);
        });
    });
};

const initHandlerSearch = () => {
    const searchInput = document.getElementById('handler-search-input');
    const searchButton = document.getElementById('handler-search-button');
    const feedback = document.getElementById('handler-search-feedback');
    const handlersSelect = document.getElementById('handlers-select');

    if (!searchInput || !searchButton || !feedback || !handlersSelect) {
        return;
    }

    const setFeedback = (message, tone = 'muted') => {
        feedback.textContent = message;
        feedback.classList.remove('text-success', 'text-danger', 'text-warning', 'text-muted');
        const toneClass = {
            success: 'text-success',
            danger: 'text-danger',
            warning: 'text-warning',
            muted: 'text-muted'
        }[tone] || 'text-muted';
        feedback.classList.add(toneClass);
    };

    const performSearch = () => {
        const rawValue = (searchInput.value || '').trim();
        if (!rawValue) {
            setFeedback('Enter an employee number to search.', 'warning');
            return;
        }

        const employeeNumber = Number(rawValue);
        if (!Number.isInteger(employeeNumber) || employeeNumber <= 0) {
            setFeedback('Employee number must be a positive integer.', 'warning');
            return;
        }

        const url = searchButton.dataset.searchUrl;
        if (!url) {
            setFeedback('Search endpoint is not configured.', 'danger');
            return;
        }

        setFeedback('Searching…', 'muted');

        fetch(`${url}?employeeNumber=${encodeURIComponent(employeeNumber)}`, {
            headers: { 'Accept': 'application/json' }
        })
            .then((response) => {
                if (!response.ok) {
                    throw new Error(`Request failed with status ${response.status}`);
                }
                return response.json();
            })
            .then((data) => {
                if (!data || data.found !== true || !data.user) {
                    const message = data && data.message ? data.message : `No handler found for employee #${employeeNumber}.`;
                    setFeedback(message, 'warning');
                    return;
                }

                const handlerId = data.user.id;
                const option = handlersSelect.querySelector(`option[value="${escapeSelector(handlerId)}"]`);

                if (option) {
                    option.selected = true;
                    handlersSelect.dispatchEvent(new Event('change', { bubbles: true }));
                    option.scrollIntoView({ block: 'nearest' });
                    handlersSelect.focus();

                    const displayName = data.user.fullName || 'Selected handler';
                    setFeedback(`Selected ${displayName} (#${data.user.employeeNumber}).`, 'success');
                }
                else {
                    setFeedback('Handler found but not available in the selection list.', 'warning');
                }
            })
            .catch(() => {
                setFeedback('Search failed. Please try again.', 'danger');
            });
    };

    searchButton.addEventListener('click', performSearch);

    searchInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            performSearch();
        }
    });

    searchInput.addEventListener('input', () => {
        if (!searchInput.value) {
            setFeedback('Enter an employee number to search.', 'muted');
        }
    });

    setFeedback('Enter an employee number to search.', 'muted');
};

document.addEventListener('DOMContentLoaded', () => {
    initPrioritySelector();
    initHandlerSearch();
});

