
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
    const handlersSelect = document.getElementById('handlers-select');

    if (!searchInput || !searchButton || !handlersSelect) {
        return;
    }

    const defaultPlaceholder = searchInput.getAttribute('placeholder') || 'Enter an employee number to search.';
    const toneClassMap = {
        success: 'is-valid',
        danger: 'is-invalid',
        warning: 'border-warning'
    };
    const toneClasses = Object.values(toneClassMap).filter(Boolean);

    const setFeedback = (message, tone = 'muted', clearValue = tone !== 'muted') => {
        const placeholderText = message || defaultPlaceholder;

        if (clearValue) {
            searchInput.value = '';
        }

        searchInput.placeholder = placeholderText;
        toneClasses.forEach((cls) => searchInput.classList.remove(cls));

        const toneClass = toneClassMap[tone];
        if (toneClass) {
            searchInput.classList.add(toneClass);
        }
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

        setFeedback('Searching...', 'muted', true);

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
            setFeedback('Enter an employee number to search.', 'muted', false);
        }
    });

    setFeedback('Enter an employee number to search.', 'muted', true);
};
const initHandlerRoleFilters = () => {
    const roleButtons = Array.from(document.querySelectorAll('.handler-role-button'));
    const handlersSelect = document.getElementById('handlers-select');

    if (!roleButtons.length || !handlersSelect) {
        return;
    }

    const handlerOptions = Array.from(handlersSelect.options);

    const setActiveButton = (activeButton) => {
        roleButtons.forEach((btn) => btn.classList.toggle('active', btn === activeButton));
    };

    const applyFilter = (allowedIds) => {
        const allowAll = !Array.isArray(allowedIds) || !allowedIds.length;
        const allowedSet = allowAll ? null : new Set(allowedIds);

        handlerOptions.forEach((option) => {
            const shouldShow = allowAll || allowedSet.has(option.value);
            option.hidden = !shouldShow;
        });

        handlersSelect.scrollTop = 0;
    };

    const showAllHandlers = () => {
        applyFilter(null);
    };

    roleButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const isReset = button.dataset.roleReset === 'true';
            if (isReset) {
                setActiveButton(button);
                showAllHandlers();
                return;
            }

            const role = button.dataset.role;
            const url = button.dataset.roleSearchUrl;
            if (!role || !url) {
                console.warn('Role filter is not configured.');
                return;
            }

            setActiveButton(button);

            fetch(`${url}?role=${encodeURIComponent(role)}`, {
                headers: { 'Accept': 'application/json' }
            })
                .then((response) => {
                    if (!response.ok) {
                        throw new Error(`Request failed with status ${response.status}`);
                    }
                    return response.json();
                })
                .then((data) => {
                    if (!data || data.found !== true || !Array.isArray(data.users) || !data.users.length) {
                        applyFilter([]); // hide all since nothing matched
                        return;
                    }

                    const allowedIds = data.users
                        .map((user) => user.id)
                        .filter((id) => typeof id === 'string' && id.length);

                    applyFilter(allowedIds);
                })
                .catch(() => {
                    console.error('Unable to load users for that role.');
                    showAllHandlers();
                });
        });
    });

    showAllHandlers();
};

const initClickableRows = () => {
    const rows = document.querySelectorAll('tr[data-href]');
    if (!rows.length) {
        return;
    }

    rows.forEach((row) => {
        row.addEventListener('click', () => {
            const target = row.dataset.href;
            if (target) {
                window.location.href = target;
            }
        });
    });
};

document.addEventListener('DOMContentLoaded', () => {
    initPrioritySelector();
    initHandlerSearch();
    initHandlerRoleFilters();
    initClickableRows();
});
