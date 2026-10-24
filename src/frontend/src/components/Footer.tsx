import LinkedInIcon from '@mui/icons-material/LinkedIn'
import { Box, Container, Link, Typography } from '@mui/material'

export function Footer() {
  return (
    <Box
      component="footer"
      sx={{
        py: 2.5,
        mt: 'auto',
        borderTop: 1,
        borderColor: 'divider',
        bgcolor: 'background.paper',
      }}
    >
      <Container maxWidth="lg">
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 0.75,
            flexWrap: 'wrap',
          }}
        >
          <Typography variant="body2" color="text.secondary">
            Site criado por
          </Typography>
          <Link
            href="https://www.linkedin.com/in/samuelalvesv/"
            target="_blank"
            rel="noopener noreferrer"
            underline="hover"
            color="primary"
            sx={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: 0.5,
              fontWeight: 600,
              fontSize: '0.875rem',
            }}
          >
            <LinkedInIcon sx={{ fontSize: 18 }} />
            Samuel Alves
          </Link>
        </Box>
      </Container>
    </Box>
  )
}
