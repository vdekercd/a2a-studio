# A2AStudio

![A2AStudio Logo](src/A2AStudio/wwwroot/logo.png)

A web application for testing Agent-to-Agent (A2A) communication using the [A2A Protocol](https://a2a-protocol.org/latest/specification/).

## ⚠️ Preview Version

This web app is in preview. Some features are missing and bugs may be found. For any remarks or issues, please [create an issue on GitHub](https://github.com/vdekercd/a2a-studio/issues).

## Features

- 🤖 **Agent Connection**: Connect to A2A-compatible agents via URL
- 💬 **Interactive Chat**: Real-time conversation interface with agents
- 📋 **Task Management**: Visual task cards showing agent task execution
- 🎨 **Dark/Light Theme**: Toggle between dark and light modes
- 📱 **Responsive Design**: Works on desktop and mobile devices
- 🔒 **Secure**: No secrets or sensitive data stored locally

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Modern web browser

### Running Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/vdekercd/a2a-studio.git
   cd a2a-studio
   ```

2. Navigate to the project directory:
   ```bash
   cd src/A2AStudio
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to `http://localhost:5000` or `https://localhost:5001`

### Building for Production

```bash
cd src/A2AStudio
dotnet publish -c Release -o publish
```

## Usage

1. **Connect to an Agent**: Enter the URL of an A2A-compatible agent in the connection form
2. **Start Chatting**: Once connected, use the chat interface to communicate with the agent
3. **View Tasks**: Task-based interactions will be displayed as interactive cards
4. **Switch Themes**: Use the theme toggle in the top-right corner

## Architecture

- **Frontend**: Blazor WebAssembly (.NET 9)
- **Styling**: Custom CSS with CSS custom properties for theming
- **A2A Integration**: Uses the [A2A NuGet package](https://www.nuget.org/packages/A2A/)
- **Deployment**: Azure Static Web Apps

## Project Structure

```
src/
├── A2AStudio/                 # Main Blazor WebAssembly project
│   ├── Components/            # Blazor components
│   │   ├── Pages/            # Page components
│   │   └── Layout/           # Layout components
│   ├── Models/               # Data models
│   ├── Services/             # Business logic and A2A integration
│   └── wwwroot/              # Static files (CSS, images, etc.)
└── Samples/
    └── EchoAgent/            # Sample A2A agent implementation
```

## Development

### Sample Agent

The project includes a sample Echo Agent in `src/Samples/EchoAgent/` that you can run to test the application:

```bash
cd src/Samples/EchoAgent
dotnet run
```

This will start the echo agent at `http://localhost:5050` by default.

### Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes and commit them: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Submit a pull request

## Technologies Used

- [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [A2A Protocol](https://a2a-protocol.org/)
- [Font Awesome](https://fontawesome.com/) for icons
- [Azure Static Web Apps](https://azure.microsoft.com/services/app-service/static/) for hosting

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Links

- [A2A Protocol Specification](https://a2a-protocol.org/latest/specification/)
- [Live Demo](https://gentle-tree-08736b303.4.azurestaticapps.net/)
- [Report Issues](https://github.com/vdekercd/a2a-studio/issues)

## Acknowledgments

- Built with the [A2A Protocol](https://a2a-protocol.org/) for agent-to-agent communication
- UI inspired by modern design systems and Tesla OS aesthetics