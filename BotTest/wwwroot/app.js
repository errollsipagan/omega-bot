const formatSelect = document.getElementById("formatSelect");
const deckSelect = document.getElementById("deckSelect");
const saveSelectionButton = document.getElementById("saveSelectionButton");
const startButton = document.getElementById("startButton");
const rematchButton = document.getElementById("rematchButton");
const resetButton = document.getElementById("resetButton");
const clearLogsButton = document.getElementById("clearLogsButton");
const selectedDeckName = document.getElementById("selectedDeckName");
const statusBadge = document.getElementById("statusBadge");
const statusDetail = document.getElementById("statusDetail");
const lastUpdated = document.getElementById("lastUpdated");
const summaryFormat = document.getElementById("summaryFormat");
const summaryDeck = document.getElementById("summaryDeck");
const summaryBusy = document.getElementById("summaryBusy");
const logList = document.getElementById("logList");
const logItemTemplate = document.getElementById("logItemTemplate");
const cardPreview = document.getElementById("cardPreview");
const cardPreviewImage = document.getElementById("cardPreviewImage");

let formats = [];
let selectedFormat = "";
let selectedDeckId = "";
let isSelectionDirty = false;
let previewHideTimeout = null;

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
    renderLogMessage(item.querySelector(".log-message"), entry.message);
    logList.appendChild(item);
  }
}

function renderLogMessage(container, message) {
  container.innerHTML = "";

  const codePattern = /\b(\d{5,10})\b/g;
  let cursor = 0;
  let match = codePattern.exec(message);

  while (match) {
    const [fullMatch, code] = match;
    const start = match.index;

    if (start > cursor) {
      container.append(document.createTextNode(message.slice(cursor, start)));
    }

    container.append(createCardCodeElement(code));
    cursor = start + fullMatch.length;
    match = codePattern.exec(message);
  }

  if (cursor < message.length) {
    container.append(document.createTextNode(message.slice(cursor)));
  }
}

function createCardCodeElement(code) {
  const element = document.createElement("span");
  element.className = "log-code";
  element.textContent = code;
  element.dataset.code = code;

  element.addEventListener("mouseenter", (event) => showCardPreview(code, event));
  element.addEventListener("mousemove", (event) => positionCardPreview(event));
  element.addEventListener("mouseleave", hideCardPreview);

  return element;
}

function showCardPreview(code, event) {
  if (previewHideTimeout) {
    window.clearTimeout(previewHideTimeout);
    previewHideTimeout = null;
  }

  cardPreviewImage.src = `https://ygopro.online/assets/card-images/common/${code}.jpg`;
  cardPreview.dataset.visible = "true";
  cardPreview.setAttribute("aria-hidden", "false");
  positionCardPreview(event);
}

function positionCardPreview(event) {
  const offset = 18;
  const previewWidth = 250;
  const previewHeight = 360;
  const maxLeft = window.innerWidth - previewWidth - 16;
  const maxTop = window.innerHeight - previewHeight - 16;
  const left = Math.min(event.clientX + offset, maxLeft);
  const top = Math.min(event.clientY + offset, maxTop);

  cardPreview.style.left = `${Math.max(16, left)}px`;
  cardPreview.style.top = `${Math.max(16, top)}px`;
}

function hideCardPreview() {
  previewHideTimeout = window.setTimeout(() => {
    cardPreview.dataset.visible = "false";
    cardPreview.setAttribute("aria-hidden", "true");
    cardPreviewImage.removeAttribute("src");
  }, 60);
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
clearLogsButton.addEventListener("click", () => postAction("/api/logs/clear").catch(showError));

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
