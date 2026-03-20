const formatSelect = document.getElementById("formatSelect");
const deckSelect = document.getElementById("deckSelect");
const saveSelectionButton = document.getElementById("saveSelectionButton");
const startButton = document.getElementById("startButton");
const rematchButton = document.getElementById("rematchButton");
const resetButton = document.getElementById("resetButton");
const selectedDeckName = document.getElementById("selectedDeckName");
const statusBadge = document.getElementById("statusBadge");
const statusDetail = document.getElementById("statusDetail");
const lastUpdated = document.getElementById("lastUpdated");
const summaryFormat = document.getElementById("summaryFormat");
const summaryDeck = document.getElementById("summaryDeck");
const summaryBusy = document.getElementById("summaryBusy");
const logList = document.getElementById("logList");
const logItemTemplate = document.getElementById("logItemTemplate");

let formats = [];
let selectedFormat = "";
let selectedDeckId = "";
let isSelectionDirty = false;

async function fetchJson(url, options) {
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...options
  });

  const payload = await response.json();
  if (!response.ok) {
    throw new Error(payload.message || "The operation failed.");
  }

  return payload;
}

function getCurrentFormat() {
  return formats.find((item) => item.id === formatSelect.value);
}

function renderFormats() {
  formatSelect.innerHTML = "";

  for (const format of formats) {
    const option = document.createElement("option");
    option.value = format.id;
    option.textContent = format.name;
    formatSelect.appendChild(option);
  }

  formatSelect.value = selectedFormat || formats[0]?.id || "";
  renderDecks();
}

function renderDecks() {
  const currentFormat = getCurrentFormat();
  deckSelect.innerHTML = "";

  if (!currentFormat) {
    return;
  }

  for (const deck of currentFormat.decks) {
    const option = document.createElement("option");
    option.value = deck.id;
    option.textContent = deck.name;
    deckSelect.appendChild(option);
  }

  const hasCurrentDeck = currentFormat.decks.some((deck) => deck.id === selectedDeckId);
  deckSelect.value = hasCurrentDeck ? selectedDeckId : currentFormat.decks[0]?.id || "";
  selectedDeckId = deckSelect.value;
}

function renderLogs(logs) {
  logList.innerHTML = "";

  if (!logs.length) {
    const empty = document.createElement("li");
    empty.className = "empty-log";
    empty.textContent = "No events recorded yet.";
    logList.appendChild(empty);
    return;
  }

  for (const entry of logs) {
    const item = logItemTemplate.content.firstElementChild.cloneNode(true);
    item.dataset.kind = entry.kind || "info";
    item.querySelector(".log-kind").textContent = entry.kind || "info";
    item.querySelector(".log-time").textContent = new Date(entry.timestamp).toLocaleTimeString("en-US");
    item.querySelector(".log-message").textContent = entry.message;
    logList.appendChild(item);
  }
}

function updateState(state) {
  if (!isSelectionDirty) {
    selectedFormat = state.selectedFormat;
    selectedDeckId = state.selectedDeckId;

    if (formatSelect.value !== selectedFormat) {
      formatSelect.value = selectedFormat;
      renderDecks();
    } else if (deckSelect.value !== selectedDeckId) {
      deckSelect.value = selectedDeckId;
    }
  }

  selectedDeckName.textContent = state.selectedDeckName || "No deck selected";
  statusBadge.textContent = state.status;
  statusDetail.textContent = state.statusDetail;
  lastUpdated.textContent = `Updated at ${new Date(state.lastUpdated).toLocaleString("en-US")}`;

  summaryFormat.textContent = getCurrentFormat()?.name || "-";
  summaryDeck.textContent = state.selectedDeckName || "-";
  summaryBusy.textContent = state.isBusy ? "Running" : "Available";

  startButton.disabled = state.isBusy;
  saveSelectionButton.disabled = state.isBusy;
  rematchButton.disabled = state.isBusy;

  renderLogs(state.logs);
}

async function loadOptions() {
  const options = await fetchJson("/api/options");
  formats = options.formats;
  selectedFormat = options.selectedFormat;
  selectedDeckId = options.selectedDeckId;
  renderFormats();
}

async function refreshState() {
  const state = await fetchJson("/api/state");
  updateState(state);
}

async function saveSelection() {
  isSelectionDirty = true;

  await fetchJson("/api/selection", {
    method: "POST",
    body: JSON.stringify({
      format: formatSelect.value,
      deckId: deckSelect.value
    })
  });

  isSelectionDirty = false;
  await refreshState();
}

async function postAction(url) {
  await fetchJson(url, { method: "POST" });
  await refreshState();
}

formatSelect.addEventListener("change", () => {
  selectedFormat = formatSelect.value;
  isSelectionDirty = true;
  renderDecks();
  saveSelection().catch(showError);
});

deckSelect.addEventListener("change", () => {
  selectedDeckId = deckSelect.value;
  isSelectionDirty = true;
  saveSelection().catch(showError);
});

saveSelectionButton.addEventListener("click", () => saveSelection().catch(showError));
startButton.addEventListener("click", () => postAction("/api/duel/start").catch(showError));
rematchButton.addEventListener("click", () => postAction("/api/duel/rematch").catch(showError));
resetButton.addEventListener("click", () => postAction("/api/duel/reset").catch(showError));

function showError(error) {
  isSelectionDirty = false;
  statusBadge.textContent = "Error";
  statusDetail.textContent = error.message;
}

async function boot() {
  await loadOptions();
  await refreshState();
  window.setInterval(() => refreshState().catch(() => {}), 2500);
}

boot().catch(showError);
