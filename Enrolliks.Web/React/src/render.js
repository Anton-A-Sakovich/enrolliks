const React = require('react');
const { createRoot } = require('react-dom/client');
const App = require('./app');

module.exports = function render(pageType) {
    const div = document.createElement('div');
    document.body.appendChild(div);
    createRoot(div).render(<App pageType={pageType} />);
}