const React = require('react');
const PersonRow = require('./personRow');
const App = require('../app');

class PeoplePage extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            content: <LoadingContent />
        };

        this.fetchPeople();
    }

    async fetchPeople() {
        try {
            const response = await window.fetch('/people/list');
            const people = await response.json();
            this.setState({
                content: people.length == 0 ? <EmptyContent /> : <PeopleContent people={people} />
            });
        } catch {
            this.setState({
                content: <ErrorContent />
            });
        }
    }

    render() {
        return (
            <div>
                <a href='/people/create'>Create new...</a>
                <div>
                    {this.state.content}
                </div>
            </div>
        );
    }
}

class LoadingContent extends React.Component {
    render() {
        return <p>Loading...</p>
    }
}

class EmptyContent extends React.Component {
    render() {
        return <p>No people found.</p>
    }
}

class ErrorContent extends React.Component {
    render() {
        return <p>Error.</p>
    }
}

class PeopleContent extends React.Component {
    render() {
        return (<table>
            <thead>
                <tr>
                    <th>Name</th>
                </tr>
            </thead>
            <tbody>
                {this.props.people.map(person => <PersonRow person={person} />)}
            </tbody>
        </table>);
    }
}

App.render(<PeoplePage />);

module.exports = PeoplePage;