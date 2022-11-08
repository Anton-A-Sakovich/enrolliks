const React = require('react');
const CreatePersonExpander = require('./createPersonExpander');
const PersonRow = require('./personRow');

const getAllPeopleResultType = {
    success: 0,
    failure: 1,
};

module.exports = class PeoplePage extends React.Component {
    render() {
        const args = this.props.args;
        const content = (args === null || !('type' in args) || args.type !== getAllPeopleResultType.success)
            ? <ErrorMessage />
            : args.people.length === 0
                ? <NoPeopleMessage />
                : <PeopleTable people={args.people} />;

        return (
            <div>
                <div>
                    <CreatePersonExpander />
                </div>
                <div className='panel'>
                    {content}
                </div>
            </div>
        );
    }
}

class NoPeopleMessage extends React.Component {
    render() {
        return <p>No people found.</p>;
    }
}

class ErrorMessage extends React.Component {
    render() {
        return <p>Error.</p>;
    }
}

class PeopleTable extends React.Component {
    render() {
        return (
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
        );
    }
}