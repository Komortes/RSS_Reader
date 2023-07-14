var deletionMode = false;
var selectedFeeds = new Set();

window.handleFeedClick = function (feedId) {
    if (deletionMode) {
        toggleFeedSelection(feedId);
    } else {
        window.location.href = '/feed/' + feedId;
    }
}

function toggleFeedSelection(feedId) {
    if (selectedFeeds.has(feedId)) {
        selectedFeeds.delete(feedId);
    } else {
        selectedFeeds.add(feedId);
    }
    updateFeedSelection(feedId);
}

function updateFeedSelection(feedId) {
    var feedCard = document.querySelector('.card[data-feed-id="' + feedId + '"]');
    var cardSelectionIcon = feedCard.querySelector('.card_selection_icon');
    if (selectedFeeds.has(feedId)) {
        feedCard.classList.add('selected');
        cardSelectionIcon.style.display = 'block';
    } else {
        feedCard.classList.remove('selected');
        cardSelectionIcon.style.display = 'none';
    }
}


