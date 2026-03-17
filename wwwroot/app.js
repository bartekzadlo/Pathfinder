document.addEventListener('DOMContentLoaded', () => {
    // Global state for swap feature
    let currentPreferences = null;
    let currentOrderedIds = [];
    let currentUnusedAttractions = [];
    let lastRouteData = null;

    const walkingDistanceInput = document.getElementById('walkingDistance');
    const distanceValue = document.getElementById('distanceValue');
    const form = document.getElementById('preferences-form');
    
    // UI Elements
    const generateBtn = document.getElementById('generate-btn');
    const loadingScreen = document.getElementById('loading');
    const resultsScreen = document.getElementById('results');
    const attractionsList = document.getElementById('attractions-list');
    const debugPanel = document.getElementById('debug-panel');
    const debugDataView = document.getElementById('debug-data-view');
    const toggleDebugBtn = document.getElementById('toggle-debug-btn');
    
    // Modal Elements
    const explainerBtn = document.getElementById('show-explainer-btn');
    const explainerModal = document.getElementById('explainer-modal');
    const closeExplainerBtn = document.getElementById('close-explainer-btn');

    // Explainer modal logic
    explainerBtn.addEventListener('click', () => {
        explainerModal.classList.remove('hidden');
    });
    
    closeExplainerBtn.addEventListener('click', () => {
        explainerModal.classList.add('hidden');
    });

    // Close modal when clicking outside
    explainerModal.addEventListener('click', (e) => {
        if (e.target === explainerModal) {
            explainerModal.classList.add('hidden');
        }
    });

    // Debug panel toggle logic
    toggleDebugBtn.addEventListener('click', () => {
        debugPanel.classList.toggle('hidden');
    });
    
    // Update distance value display
    walkingDistanceInput.addEventListener('input', (e) => {
        distanceValue.textContent = `${e.target.value} km`;
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Gather preferences
        currentPreferences = {
            city: document.getElementById('city').value,
            walkingDistanceKm: parseFloat(document.getElementById('walkingDistance').value),
            transportMode: document.getElementById('transportMode').value,
            weather: document.getElementById('weather').value,
            focusType: parseInt(document.getElementById('focusType').value)
        };

        // Show loading state, hide form and results
        form.classList.add('hidden');
        resultsScreen.classList.add('hidden');
        loadingScreen.classList.remove('hidden');

        try {
            const response = await fetch('/api/route', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(currentPreferences)
            });

            if (!response.ok) {
                throw new Error('API Error');
            }

            const data = await response.json();
            
            // Artificial delay to show off loading animation
            setTimeout(() => {
                displayResults(data);
            }, 800);

        } catch (error) {
            console.error('Failed to fetch route', error);
            alert('Wystąpił błąd podczas generowania trasy. Spróbuj ponownie później.');
            // Reset UI
            loadingScreen.classList.add('hidden');
            form.classList.remove('hidden');
        }
    });

    function displayResults(data) {
        lastRouteData = data;
        loadingScreen.classList.add('hidden');
        resultsScreen.classList.remove('hidden');

        // Extract ordered IDs for swapping
        currentOrderedIds = [];
        if (data.startAttraction) currentOrderedIds.push(data.startAttraction.id);
        if (data.segments) {
            data.segments.forEach(seg => currentOrderedIds.push(seg.toAttraction.id));
        }
        currentUnusedAttractions = data.unusedAttractions || [];

        document.getElementById('res-distance').textContent = `${data.totalDistanceKm.toFixed(1)} km`;
        
        // Format time properly
        const hours = Math.floor(data.totalEstimatedTimeMinutes / 60);
        const minutes = data.totalEstimatedTimeMinutes % 60;
        let timeStr = '';
        if (hours > 0) timeStr += `${hours}h `;
        timeStr += `${minutes}min`;
        
        document.getElementById('res-time').textContent = timeStr;

        // Render timeline
        attractionsList.innerHTML = '';
        
        if (!data.startAttraction) {
            attractionsList.innerHTML = '<li><p>Niestety, nie znaleźliśmy atrakcji pasujących do Twoich wymagań.</p></li>';
            return;
        }

        // Render Start Attraction
        const startLi = renderAttractionLi(data.startAttraction, 0);
        attractionsList.appendChild(startLi);

        // Render Segments
        data.segments.forEach((seg, idx) => {
            // Render Transport connection
            const connectionDiv = document.createElement('div');
            connectionDiv.className = 'transport-connection';
            connectionDiv.style.animation = `fadeIn 0.5s ease-out ${(idx * 0.15) + 0.1}s both`;
            connectionDiv.innerHTML = `
                <div class="transport-icon">${seg.transportModeIcon}</div>
                <div class="transport-details">
                    <span>${seg.travelDistanceKm.toFixed(1)} km</span>
                    <span>${seg.travelTimeMinutes} min</span>
                </div>
            `;
            attractionsList.appendChild(connectionDiv);

            // Render next Attraction
            const destLi = renderAttractionLi(seg.toAttraction, idx + 1);
            attractionsList.appendChild(destLi);
        });

        // Populate debug data if exists
        if (data.debugData) {
            let tableHTML = `
                <div style="margin-bottom: 1rem;">
                    <strong>Współczynnik Eksploracji (ExploreWeight):</strong> ${data.debugData.calculatedExploreWeight.toFixed(2)}<br>
                    <strong>Współczynnik Relaksu (RelaxWeight):</strong> ${data.debugData.calculatedRelaxWeight.toFixed(2)}<br>
                    <strong>Domyślne tempo podróży:</strong> ${data.debugData.transportAssumptions.speedKmPerHour} km/h (${data.debugData.transportAssumptions.baseIcon})<br>
                    <strong>Ilość odcinków w trasie:</strong> ${data.debugData.transportAssumptions.segmentCount}
                </div>
                <table style="width: 100%; border-collapse: collapse; text-align: left;">
                    <thead>
                        <tr style="border-bottom: 1px solid rgba(255,255,255,0.2);">
                            <th style="padding: 0.5rem;">Atrakcja</th>
                            <th style="padding: 0.5rem;">Oryg. Eksploracja</th>
                            <th style="padding: 0.5rem;">Oryg. Relaks</th>
                            <th style="padding: 0.5rem; color: var(--secondary);">Wynik Końcowy (Score)</th>
                        </tr>
                    </thead>
                    <tbody>
            `;
            
            data.debugData.allScoredAttractions.forEach(item => {
                tableHTML += `
                    <tr style="border-bottom: 1px solid rgba(255,255,255,0.05);">
                        <td style="padding: 0.5rem;">${item.name} ${item.isOutdoor ? '🌳' : '🏛️'}</td>
                        <td style="padding: 0.5rem;">${item.originalExploration}/10</td>
                        <td style="padding: 0.5rem;">${item.originalRelaxation}/10</td>
                        <td style="padding: 0.5rem; font-weight: bold; color: var(--secondary);">${item.calculatedScore.toFixed(2)}</td>
                    </tr>
                `;
            });
            
            tableHTML += `</tbody></table>`;
            debugDataView.innerHTML = tableHTML;
        } else {
            debugDataView.innerHTML = "<p>Brak danych debuggera.</p>";
        }

        // Add a "Start Over" button at the bottom
        const startOverBtn = document.createElement('button');
        startOverBtn.className = 'primary-btn';
        startOverBtn.style.marginTop = '2rem';
        startOverBtn.style.background = 'rgba(255,255,255,0.1)';
        startOverBtn.style.boxShadow = 'none';
        startOverBtn.textContent = 'Zaplanuj Nową Trasę 🔄';
        startOverBtn.onclick = () => {
            resultsScreen.classList.add('hidden');
            form.classList.remove('hidden');
            // reset form button animation
            form.style.animation = 'none';
            form.offsetHeight; /* trigger reflow */
            form.style.animation = null; 
        };
        attractionsList.appendChild(startOverBtn);
    }

    // Helper to render attraction LI with swap logic
    function renderAttractionLi(attraction, index) {
        const li = document.createElement('li');
        li.style.animation = `fadeIn 0.5s ease-out 0s both`;
        
        const headerDiv = document.createElement('div');
        headerDiv.className = 'attraction-header';
        headerDiv.innerHTML = `<h3>${index + 1}. ${attraction.name} ${attraction.isOutdoor ? '🌳' : '🏛️'}</h3>`;
        
        const swapBtn = document.createElement('button');
        swapBtn.className = 'secondary-btn swap-btn';
        swapBtn.innerHTML = '🔄 Zamień';
        swapBtn.onclick = () => showSwapUI(li, attraction, index);
        
        headerDiv.appendChild(swapBtn);
        li.appendChild(headerDiv);

        const descP = document.createElement('p');
        descP.innerHTML = `<span>Czas spędzony: ${attraction.recommendedDurationMinutes} min</span>`;
        li.appendChild(descP);
        
        return li;
    }

    function showSwapUI(liElement, currentAttraction, index) {
        let optionsHtml = '<option value="">-- Wybierz inne miejsce --</option>';
        currentUnusedAttractions.forEach(attr => {
            optionsHtml += `<option value="${attr.id}">${attr.name} (${attr.isOutdoor ? 'Plener' : 'Budynek'})</option>`;
        });

        liElement.innerHTML = `
            <div style="padding: 1rem; background: rgba(0,0,0,0.3); border-radius: 8px;">
                <p style="margin-bottom: 0.5rem; color: #fff; font-size: 0.9rem;">Zamieniasz: <strong>${currentAttraction.name}</strong></p>
                <select id="swap-select-${index}" class="swap-select">
                    ${optionsHtml}
                </select>
                <div style="display: flex; gap: 0.5rem;">
                    <button id="confirm-swap-${index}" class="primary-btn" style="flex: 1; padding: 0.5rem; font-size: 0.85rem;">Zapisz</button>
                    <button id="cancel-swap-${index}" class="secondary-btn" style="flex: 1; padding: 0.5rem; font-size: 0.85rem;">Anuluj</button>
                </div>
            </div>
        `;

        document.getElementById(`cancel-swap-${index}`).onclick = () => {
            displayResults(lastRouteData);
        };

        document.getElementById(`confirm-swap-${index}`).onclick = () => {
            const select = document.getElementById(`swap-select-${index}`);
            const newId = parseInt(select.value);
            if(!newId) return;

            const newOrderedIds = [...currentOrderedIds];
            newOrderedIds[index] = newId;
            fetchRecalculatedRoute(newOrderedIds);
        };
    }

    async function fetchRecalculatedRoute(newIds) {
        loadingScreen.classList.remove('hidden');
        resultsScreen.classList.add('hidden');

        try {
            const payload = {
                preferences: currentPreferences,
                attractionIds: newIds
            };
            const response = await fetch('/api/route/recalculate', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify(payload)
            });
            
            if(!response.ok) throw new Error('API Error');
            const newData = await response.json();
            
            displayResults(newData);
        } catch (error) {
            console.error('Failed to recalculate', error);
            alert('Wystąpił błąd podczas zamiany.');
            displayResults(lastRouteData); // restore
        }
    }
});
