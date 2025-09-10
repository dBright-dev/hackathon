
# Hackathon Project: Event Management for Educational Institutions

## Project Overview

This web application serves as a comprehensive platform for educational institutions to manage events, workshops, and meetups. The goal is to facilitate better communication and engagement by allowing students and faculty to create, manage, and participate in events.

The application was developed by Bolu and Nkumbu as a submission for a hackathon.

## Features

The application includes the following core functionalities:

- **User  Authentication:**  
  Secure user registration and login with email verification and password handling. It also includes password recovery and reset options, and role management for students and faculty.

- **Event Management:**  
  A system for creating, editing, and deleting events with fields for a title, description, date, time, location, and capacity. Users can also view event details and a list of all upcoming events.

- **RSVP Functionality:**  
  Users can RSVP to events to track attendance. Organizers receive notifications upon RSVP to help manage resources.

- **Calendar View:**  
  A calendar view that displays all upcoming events, which users can click on to view details or RSVP. Events can be filtered by category or date.

- **Material Sharing:**  
  The ability to upload documents or links related to specific events. Users can view and download these materials.

## Getting Started

### Prerequisites

To run this project, you will need to have the following installed:

- Visual Studio or Visual Studio Code  
- .NET Core SDK  
- SQL Server  
- Git  

### Installation

1. Clone the repository:  
   ```bash
   git clone [repository URL]
   cd [repository name]
   ```

2. Set up the database:  
   - Open SQL Server and create a new database.  
   - Update the connection string in `appsettings.json` to point to your new database.  
   - Run Entity Framework Core migrations to create the necessary tables for users, events, RSVPs, and materials.

3. Run the application:  
   - Open the project in Visual Studio or Visual Studio Code.  
   - Run the application from the IDE. The frontend will be served, and the backend API will start.

## Technology Stack

### Frontend

- **HTML5:** For structuring the web pages.  
- **CSS (with Bootstrap):** For styling and responsive design.  
- **JavaScript:** For interactivity, with the option to use a framework like React or Vue.js.

### Backend

- **C# with ASP.NET Core:** For server-side programming.  
- **ASP.NET Core Identity:** For user authentication and role management.  
- **Entity Framework Core:** For database interactions.

### Database

- **SQL Server:** Used to store all application data, including user information, events, RSVPs, and shared materials.

## Authors

- Bolu  
- Nkumbu
- Mako
