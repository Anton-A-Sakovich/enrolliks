const React = require('react');
const App = require('../app');
const CreatePersonComponent = require('./createPersonComponent');
const PersonRow = require('./personRow');
const getData = require('../getData');

const success = 0;
const failure = 1;

class IndexPage extends React.Component {
    constructor(props) {
        super(props);

        let content;
        const data = getData();
        if (data === null || !('type' in data) || data.type === failure)
            content = <ErrorContent />;
        else if (data.type === success)
            content = data.people.length === 0
                ? <EmptyContent />
                : <PeopleContent people={data.people} />;
        else
            content = <ErrorContent />;

        this.state = {
            content: content,
        };
    }

    render() {
        return (
            <div>
                <div>
                    <CreatePersonComponent />
                </div>
                <div>
                    {this.state.content}
                </div>
            </div>
        );
    }
}

class EmptyContent extends React.Component {
    render() {
        return <p>No people found.</p>;
    }
}

class ErrorContent extends React.Component {
    render() {
        return <p>Error.</p>;
    }
}

class PeopleContent extends React.Component {
    render() {
        return (
        <div className='panel'>
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {this.props.people.map(person => <PersonRow key={person.name} person={person} />)}
                </tbody>
            </table>
        </div>);
    }
}

App.definePage(IndexPage);
module.exports = IndexPage;