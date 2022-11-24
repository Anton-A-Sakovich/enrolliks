const React = require('react');
const { Outlet, Link } = require('react-router-dom');

module.exports = class Root extends React.Component {
    render() {
        return (<>
            <h1>Enrolliks</h1>
            <nav className='top-nav'>
                <Link to='home'>Home</Link>
                <Link to='people'>People</Link>
            </nav>
            <div className='content-area'>
                <Outlet />
            </div>
        </>);
    }
}