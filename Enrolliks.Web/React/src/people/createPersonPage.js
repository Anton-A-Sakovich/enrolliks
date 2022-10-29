const React = require('react');
const axios = require('axios').default;
const App = require('../app');

class CreatePersonPage extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            name: '',
            submitAllowed: true,
            error: false
        };
    }

    async submit() {
        try {
            this.setState({submitAllowed: false});

            await axios.post('/people/create', {
                name: this.state.name
            });

            window.location.href = window.origin + '/people';
        } catch {
            this.setState({submitAllowed: true, error: true});
        }
    }
    
    render() {
        const errorStyle = {
            display: this.state.error ? 'inline' : 'none'
        };

        const handleNameChange = event => this.setState({name: event.target.value});
        const handleButtonClick = () => this.submit();

        return (
            <div>
                <label>Name</label>
                <span style={errorStyle}>Error</span>
                <input type={'text'} value={this.state.name} onChange={handleNameChange}></input>
                <button onClick={handleButtonClick} disabled={!this.state.submitAllowed}>Create</button>
            </div>
        );
    }
}

App.render(<CreatePersonPage />);

module.exports = CreatePersonPage;